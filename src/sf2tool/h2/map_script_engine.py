from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.entity_action_scripts import _access_rows, _global_access_rows
from sf2tool.h2.map_content import build_map_content_contract
from sf2tool.h2.sprite_dialogue import build_sprite_dialogue_contract
from sf2tool.h2.stats import build_stats_inventory
from sf2tool.h2.text_banks import build_text_line_domain_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-script-engine-static-v1"
MANIFEST = repo_path("manifests/extractions/map-script-engine-static.json")
SCHEMA = repo_path("schemas/map-script-engine-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")

MACRO_PATH = Path("sf2cutscenemacros.asm")
ENGINE_PATHS = (
    Path("code/common/scripting/map/mapscriptengine_1.asm"),
    Path("code/common/scripting/map/mapscriptengine_2.asm"),
)
DISPATCH_SOURCE = ENGINE_PATHS[1]

DIALOGUE_MACROS = (
    "nextSingleText",
    "nextSingleTextVar",
    "nextText",
    "nextTextVar",
    "textCursor",
    "hideText",
)
DIALOGUE_DISPLAY_MACROS = DIALOGUE_MACROS[:4]
DIALOGUE_HANDLER_BY_MACRO = {
    "nextSingleText": "csc00_displaySingleTextbox",
    "nextSingleTextVar": "csc01_displaySingleTextboxWithVars",
    "nextText": "csc02_displayTextbox",
    "nextTextVar": "csc03_displayTextboxWithVars",
    "textCursor": "csc04_setTextIndex",
    "hideText": "csc09_hideDialogueAndPortraitWindows",
}
DIALOGUE_MODIFIER_MACROS = ("nextSingleText", "nextText")
PORTRAIT_HANDLER = "csc1D_showPortrait"
ENTITY_DIALOGUE_CONSUMER_PATH = Path(
    "code/common/scripting/entity/getentityportaitandspeechsfx.asm"
)
ENTITY_DIALOGUE_CONSUMER = "GetEntityPortaitAndSpeechSfx"
DIALOGUE_CALLER_HANDLER_NAMES = tuple(
    DIALOGUE_HANDLER_BY_MACRO[macro] for macro in DIALOGUE_MACROS
) + (PORTRAIT_HANDLER,)
DIALOGUE_CALLEE_TARGETS = (PORTRAIT_HANDLER, ENTITY_DIALOGUE_CONSUMER)
DIALOGUE_CONSTANT_NAMES = ("COMBATANT_MASK_ALL", "ENTITY_NONE")
DIALOGUE_RUNTIME_QUESTIONS = ["dialogue-presentation/runtime-matrix"]

TRANSITION_MACROS = ("warp", "resetMap", "loadMapFadeIn", "reloadMap", "mapLoad")
TRANSITION_HANDLER_BY_MACRO = {
    "warp": "csc07_warp",
    "resetMap": "csc36_resetMap",
    "loadMapFadeIn": "csc37_loadMapAndFadeIn",
    "reloadMap": "csc46_reloadMap",
    "mapLoad": "csc48_loadMap",
}
TRANSITION_SERVICE_TARGETS = (
    "ResetCurrentMap",
    "LoadMapTilesets",
    "LoadMap",
    "EnableDisplayAndInterrupts",
)
TRANSITION_RUNTIME_QUESTIONS = ["map-script-transition-presentation-matrix"]

# These source-faithful macro names delimit the force-state slice. Behavior is
# reconstructed from their handler sections; macro names alone are not a
# semantic interpretation.
FORCE_STATE_MACRO_NAMES = (
    "join",
    "jumpIfDefeatedByLastAttack",
    "jumpIfDead",
    "allyDefeated",
    "updateDefeatedAllies",
    "reviveAlly",
)

FORCE_STATE_HANDLER_NAMES = (
    "csc08_joinForce",
    "csc0E_jumpIfForceMemberInList",
    "csc0F_jumpIfCharacterDead",
    "csc1F_addDefeatedAlly",
    "csc20_updateDefeatedAllies",
    "csc21_reviveAlly",
)
FORCE_STATE_RUNTIME_QUESTIONS = ["force-state/roster-death-persistence-visible-outcomes"]

# This adjacent source-faithful macro group manipulates active-party membership,
# activation state, battle-stat reset, or follower selection.  The labels remain
# macro labels until the grouped runtime question establishes player-visible
# effects.
ACTIVE_PARTY_MACRO_NAMES = (
    "joinBatParty",
    "joinForceAI",
    "resetForceBattleStats",
    "addNewFollower",
)

ACTIVE_PARTY_HANDLER_NAMES = (
    "csc51_joinBattleParty",
    "csc54_joinForceAi",
    "csc55_resetCharacterBattleStats",
    "csc56_addFollower",
)
ACTIVE_PARTY_RUNTIME_QUESTIONS = ["force-state/active-party-ai-follower-runtime-matrix"]

# Source macro labels distinguish the primary $10 carrier from its two fixed-
# operand aliases.  This slice preserves those identities without inferring a
# persistence model from their names.
STORY_STATE_MACRO_NAMES = (
    "jumpIfFlagSet",
    "jumpIfFlagClear",
    "csc10",
    "setF",
    "clearF",
    "yesNo",
    "setStoryFlag",
)
STORY_STATE_HANDLER_BY_MACRO = {
    "jumpIfFlagSet": "csc0C_jumpIfFlagSet",
    "jumpIfFlagClear": "csc0D_jumpIfFlagClear",
    "csc10": "csc10_toggleFlag",
    "setF": "csc10_toggleFlag",
    "clearF": "csc10_toggleFlag",
    "yesNo": "csc11_promptYesNoForStoryFlow",
    "setStoryFlag": "csc13_setStoryFlag",
}
STORY_STATE_HANDLER_NAMES = (
    "csc0C_jumpIfFlagSet",
    "csc0D_jumpIfFlagClear",
    "csc10_toggleFlag",
    "csc11_promptYesNoForStoryFlow",
    "csc13_setStoryFlag",
)
STORY_STATE_RUNTIME_QUESTIONS = ["story-state/branch-prompt-persistence-matrix"]

# Source labels describe the emitted six-byte block-copy operand payload without
# assigning persistence, collision, or visible presentation semantics.
MAP_BLOCK_MUTATION_MACRO_NAMES = ("setBlocks", "setBlocksVar")
MAP_BLOCK_MUTATION_HANDLER_BY_MACRO = {
    "setBlocks": "csc34_setBlocks",
    "setBlocksVar": "csc35_setBlocksVar",
}
MAP_BLOCK_MUTATION_HANDLER_NAMES = ("csc34_setBlocks", "csc35_setBlocksVar")
MAP_BLOCK_MUTATION_RUNTIME_QUESTIONS = ["map-block-mutation/runtime-effects-matrix"]

# These names retain the macro/handler spelling without assigning allocation,
# persistence, collision, or presentation behavior to the commands.
ENTITY_POPULATION_MACRO_NAMES = (
    "newEntity",
    "loadMapEntities",
    "reloadEntities",
    "loadEntitiesFromMapSetup",
)
ENTITY_POPULATION_HANDLER_BY_MACRO = {
    "newEntity": "csc2B_initializeNewEntity",
    "loadMapEntities": "csc42_loadMapEntities",
    "reloadEntities": "csc44_reloadEntities",
    "loadEntitiesFromMapSetup": "csc49_loadEntitiesFromMapSetup",
}
ENTITY_POPULATION_HANDLER_NAMES = tuple(ENTITY_POPULATION_HANDLER_BY_MACRO.values())
ENTITY_POPULATION_RUNTIME_QUESTIONS = [
    "entity-population-reload/runtime-effects-matrix"
]

# These names retain the macro/handler spelling and source operand comments. They
# describe the bounded map-load control-flow surface without assigning persistence,
# placement, collision, rendering, or presentation behavior.
MAP_LIFECYCLE_MACRO_NAMES = (
    "resetMap",
    "loadMapFadeIn",
    "reloadMap",
    "mapLoad",
)
MAP_LIFECYCLE_HANDLER_BY_MACRO = {
    "resetMap": "csc36_resetMap",
    "loadMapFadeIn": "csc37_loadMapAndFadeIn",
    "reloadMap": "csc46_reloadMap",
    "mapLoad": "csc48_loadMap",
}
MAP_LIFECYCLE_HANDLER_NAMES = tuple(MAP_LIFECYCLE_HANDLER_BY_MACRO.values())
MAP_LIFECYCLE_RUNTIME_QUESTIONS = ["map-lifecycle/runtime-effects-matrix"]


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _literal(value: str) -> int:
    value = value.strip()
    return int(value[1:], 16) if value.startswith("$") else int(value)


def _macro_blocks(source: str) -> dict[str, str]:
    pattern = re.compile(
        r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s*macro[^\n]*\n"
        r"(?P<body>.*?)(?=^\s*endm\s*$)",
        re.MULTILINE | re.DOTALL,
    )
    return {match.group("name"): match.group("body") for match in pattern.finditer(source)}


def _emission_rows(body: str) -> list[dict[str, Any]]:
    widths = {"b": 1, "w": 2, "l": 4}
    rows: list[dict[str, Any]] = []
    offset = 0
    for raw_line in body.splitlines():
        code = raw_line.split(";", 1)[0].strip()
        direct = re.match(r"^dc\.([bwl])\s+(.+)$", code)
        shorthand = re.match(
            r"^defineShorthand\.([bwl])\s+([^,]+),(.+)$", code
        )
        if direct:
            width_code, expression = direct.groups()
            encoding = "direct"
        elif shorthand:
            width_code, prefix, expression = shorthand.groups()
            encoding = f"shorthand:{prefix.strip()}"
        else:
            continue
        expression = expression.strip()
        if direct and "," in expression:
            raise ValueError(f"unsupported multi-value map-script emission: {code}")
        width = widths[width_code]
        rows.append(
            {
                "streamOffset": offset,
                "widthBytes": width,
                "expression": expression,
                "parameterOrdinals": sorted(
                    {int(value) for value in re.findall(r"\\(\d+)", expression)}
                ),
                "encoding": encoding,
            }
        )
        offset += width
    return rows


def _substitute_alias_layout(
    layout: list[dict[str, Any]], arguments: list[str]
) -> list[dict[str, Any]]:
    def substitute(match: re.Match[str]) -> str:
        ordinal = int(match.group(1))
        if ordinal > len(arguments):
            raise ValueError(f"map-script alias argument {ordinal} is missing")
        return arguments[ordinal - 1]

    rows = []
    for row in layout:
        expression = re.sub(r"\\(\d+)", substitute, row["expression"])
        rows.append(
            {
                **row,
                "expression": expression,
                "parameterOrdinals": sorted(
                    {int(value) for value in re.findall(r"\\(\d+)", expression)}
                ),
            }
        )
    return rows


def _map_macro_contracts(disasm: Path) -> dict[str, dict[str, Any]]:
    source = read_upstream_text(disasm / MACRO_PATH)
    prefix = source.split("; entity data structure", 1)[0]
    blocks = _macro_blocks(prefix)
    primary: dict[str, dict[str, Any]] = {}
    for name, body in blocks.items():
        emissions = _emission_rows(body)
        first_word = re.search(
            r"^\s*dc\.w\s+(\$?[0-9A-Fa-f]+)\b", body, re.MULTILINE
        )
        if first_word is None:
            continue
        opcode = _literal(first_word.group(1))
        if opcode <= 0x56:
            if not emissions or emissions[0]["widthBytes"] != 2:
                raise ValueError(f"map-script opcode emission is malformed: {name}")
            operand_layout = emissions[1:]
            parameters = sorted(
                {int(value) for value in re.findall(r"\\(\d+)", body)}
            )
            primary[name] = {
                "kind": "command",
                "opcode": opcode,
                "encodedBytes": sum(row["widthBytes"] for row in emissions),
                "operandBytes": sum(row["widthBytes"] for row in operand_layout),
                "operandLayout": operand_layout,
                "parameterOrdinals": parameters,
                "aliasOf": None,
            }
    if len(primary) != 82 or len({row["opcode"] for row in primary.values()}) != 82:
        raise ValueError("map-script primary macro boundary drift")

    aliases: dict[str, dict[str, Any]] = {}
    for name, body in blocks.items():
        if name in primary:
            continue
        call = re.search(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\b", body, re.MULTILINE)
        if call and call.group(1) in primary:
            target = call.group(1)
            call_line = next(
                line.split(";", 1)[0].strip()
                for line in body.splitlines()
                if line.split(";", 1)[0].strip()
            )
            argument_text = call_line[len(target) :].strip()
            arguments = [value.strip() for value in argument_text.split(",")]
            operand_layout = _substitute_alias_layout(
                primary[target]["operandLayout"], arguments
            )
            aliases[name] = {
                **primary[target],
                "operandLayout": operand_layout,
                "parameterOrdinals": sorted(
                    {int(value) for value in re.findall(r"\\(\d+)", body)}
                ),
                "aliasOf": target,
            }
    if len(aliases) != 8:
        raise ValueError("map-script alias macro boundary drift")

    special_kinds = {
        "csWait": "sleep",
        "cscNop": "source-nop",
        "csc_end": "terminator",
    }
    if not set(special_kinds).issubset(blocks):
        raise ValueError("map-script special macro boundary drift")
    special = {}
    for name, kind in special_kinds.items():
        emissions = _emission_rows(blocks[name])
        operand_layout = emissions[1:] if name == "csWait" else []
        special[name] = {
            "kind": kind,
            "opcode": None,
            "encodedBytes": sum(row["widthBytes"] for row in emissions),
            "operandBytes": sum(row["widthBytes"] for row in operand_layout),
            "operandLayout": operand_layout,
            "parameterOrdinals": sorted(
                {int(value) for value in re.findall(r"\\(\d+)", blocks[name])}
            ),
            "aliasOf": None,
        }
    return {**primary, **aliases, **special}


def _dispatch_targets(source: str) -> list[str]:
    table = re.search(
        r"^rjt_cutsceneScriptCommands:\s*\n(?P<body>.*?)(?=^loc_47234:)",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if table is None:
        raise ValueError("map-script dispatcher table is missing")
    targets = re.findall(
        r"^\s*dc\.w\s+\(?([A-Za-z_][A-Za-z0-9_]*)-rjt_cutsceneScriptCommands\)?",
        table.group("body"),
        re.MULTILINE,
    )
    if len(targets) != 90:
        raise ValueError(f"map-script dispatcher slot drift: {len(targets)}")
    return targets


def _statements(body: str) -> list[str]:
    statements = []
    for raw_line in body.splitlines():
        code = raw_line.split(";", 1)[0].strip()
        if not code or re.match(r"^[A-Za-z_@][A-Za-z0-9_@]*:$", code):
            continue
        if re.match(r"^[a-z][A-Za-z0-9]*(?:\.[bwls])?(?:\s+|$)", code):
            statements.append(re.sub(r"\s+", " ", code))
    return statements


def _handler_family(opcode: int, target: str) -> str:
    if target == "csc_doNothing":
        return "filler"
    if opcode <= 4:
        return "text"
    if opcode == 5:
        return "audio"
    if opcode == 6:
        return "no-op"
    if opcode == 7:
        return "map-transition"
    if opcode == 8:
        return "party"
    if opcode == 9:
        return "dialogue-ui"
    if 10 <= opcode <= 15:
        return "control-flow"
    if 16 <= opcode <= 19:
        return "story-state"
    if 20 <= opcode <= 49:
        return "entity"
    if 50 <= opcode <= 55:
        return "map-camera"
    if 57 <= opcode <= 65 or 74 <= opcode <= 75:
        return "presentation"
    if 66 <= opcode <= 73:
        return "map-state"
    if 80 <= opcode <= 86:
        return "entity-party"
    raise ValueError(f"unclassified map-script handler opcode: {opcode}")


def _cursor_flow(target: str, statements: list[str]) -> str:
    has_absolute_transfer = "movea.l (a6),a6" in statements
    has_skip = "addq.w #4,a6" in statements
    if has_absolute_transfer and has_skip:
        return "conditional-absolute-jump"
    if has_absolute_transfer:
        return "absolute-jump"
    if target == "csc14_setEntityActscriptManual":
        if (
            "move.l a6,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)" not in statements
            or "cmpi.w #$8080,(a6)+" not in statements
        ):
            raise ValueError("map-script inline action-program cursor shape drift")
        return "inline-action-program"
    return "sequential"


def _handler_rows(
    disasm: Path,
    addresses: dict[str, int],
    dispatch_targets: list[str],
    source_counts: Counter[str],
    macro_contracts: dict[str, dict[str, Any]],
) -> list[dict[str, Any]]:
    sources = {path: read_upstream_text(disasm / path) for path in ENGINE_PATHS}
    macros_by_opcode: dict[int, list[str]] = {}
    for name, contract in macro_contracts.items():
        if contract["opcode"] is not None:
            macros_by_opcode.setdefault(contract["opcode"], []).append(name)
    rows = []
    for target in sorted(set(dispatch_targets)):
        owner = None
        body_match = None
        for path, source in sources.items():
            match = re.search(
                rf"^{re.escape(target)}:\s*\n(?P<body>.*?)"
                rf"^\s*; End of function {re.escape(target)}\s*$",
                source,
                re.MULTILINE | re.DOTALL,
            )
            if match:
                owner, body_match = path, match
                break
        if owner is None or body_match is None:
            raise ValueError(f"map-script handler body is missing: {target}")
        if target not in addresses:
            raise ValueError(f"map-script handler lacks H1 address: {target}")
        source = sources[owner]
        body = body_match.group("body")
        statements = _statements(body)
        opcodes = [index for index, value in enumerate(dispatch_targets) if value == target]
        entity_accesses = _access_rows(
            statements, re.compile(r"\b(ENTITYDEF_OFFSET_[A-Z0-9_]+)\b")
        )
        direct_calls = sorted(
            set(
                re.findall(
                    r"\b(?:bsr|jsr)(?:\.[bwl])?\s+\(?([A-Za-z_][A-Za-z0-9_]*)",
                    body,
                )
            )
        )
        macro_names = sorted(
            {name for opcode in opcodes for name in macros_by_opcode.get(opcode, [])}
        )
        encoded_sizes = {macro_contracts[name]["encodedBytes"] for name in macro_names}
        operand_sizes = {macro_contracts[name]["operandBytes"] for name in macro_names}
        if len(encoded_sizes) > 1 or len(operand_sizes) > 1:
            raise ValueError(f"map-script alias physical layout drift: {target}")
        rows.append(
            {
                "name": target,
                "opcodes": opcodes,
                "families": sorted({_handler_family(opcode, target) for opcode in opcodes}),
                "address": addresses[target],
                "sourcePath": owner.as_posix(),
                "startLine": source.count("\n", 0, body_match.start("body")) + 1,
                "endLine": source.count("\n", 0, body_match.end("body")) + 1,
                "statementCount": len(statements),
                "macroNames": macro_names,
                "encodedCommandBytes": next(iter(encoded_sizes), 2),
                "operandBytes": next(iter(operand_sizes), 0),
                "cursorFlow": _cursor_flow(target, statements),
                "sourceCommandCount": sum(source_counts[name] for name in macro_names),
                "entityFieldAccesses": entity_accesses,
                "globalStateAccesses": _global_access_rows(statements),
                "directCalls": direct_calls,
                "scriptCursorStatements": [row for row in statements if "a6" in row],
            }
        )
    if len(rows) != 83:
        raise ValueError(f"map-script unique handler boundary drift: {len(rows)}")
    return rows


def _source_usage(
    disasm: Path, macro_contracts: dict[str, dict[str, Any]]
) -> tuple[Counter[str], dict[str, int], list[str]]:
    counts: Counter[str] = Counter()
    paths: set[str] = set()
    pattern = re.compile(
        r"^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?([A-Za-z_][A-Za-z0-9_]*)\b"
    )
    for root_name in ("code", "data"):
        for path in sorted((disasm / root_name).rglob("*.asm")):
            relative = path.relative_to(disasm)
            source = read_upstream_text(path)
            found = False
            for raw_line in source.splitlines():
                match = pattern.match(raw_line.split(";", 1)[0])
                if match and match.group(1) in macro_contracts:
                    counts[match.group(1)] += 1
                    found = True
            if found:
                paths.add(relative.as_posix())
    opcode_counts: Counter[int] = Counter()
    for name, count in counts.items():
        opcode = macro_contracts[name]["opcode"]
        if opcode is not None:
            opcode_counts[opcode] += count
    for name in macro_contracts:
        counts.setdefault(name, 0)
    return (
        counts,
        {str(key): value for key, value in sorted(opcode_counts.items())},
        sorted(paths),
    )


def _logical_source_lines(source: str) -> list[tuple[int, str]]:
    rows: list[tuple[int, str]] = []
    pending = ""
    start_line = 0
    for line_number, raw_line in enumerate(source.splitlines(), start=1):
        code = raw_line.split(";", 1)[0].rstrip()
        if not code.strip():
            continue
        if pending:
            code = f"{pending} {code.strip()}"
        else:
            start_line = line_number
        if code.rstrip().endswith("&"):
            pending = code.rstrip()[:-1].rstrip()
            continue
        rows.append((start_line, code))
        pending = ""
    if pending:
        raise ValueError("unterminated map-script source continuation")
    return rows


def _invocation(
    statement: str, macro_contracts: dict[str, dict[str, Any]]
) -> tuple[str, list[str]] | None:
    match = re.match(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\b(.*)$", statement)
    if not match or match.group(1) not in macro_contracts:
        return None
    argument_text = match.group(2).strip()
    arguments = (
        [argument.strip() for argument in argument_text.split(",")]
        if argument_text
        else []
    )
    return match.group(1), arguments


def _target_symbol(expression: str, known_symbols: set[str]) -> str:
    candidates = re.findall(r"\b[A-Za-z_][A-Za-z0-9_]*\b", expression)
    matches = [candidate for candidate in candidates if candidate in known_symbols]
    if len(matches) != 1:
        raise ValueError(f"map-script target expression is ambiguous: {expression}")
    return matches[0]


def _program_references(
    disasm: Path,
    label_owners: dict[str, str],
    programs: list[dict[str, Any]],
) -> dict[str, Any]:
    occurrences = {label: Counter() for label in label_owners}
    scanned_files = 0
    for root_name in ("code", "data"):
        for path in sorted((disasm / root_name).rglob("*.asm")):
            scanned_files += 1
            source_path = path.relative_to(disasm).as_posix()
            for raw_line in read_upstream_text(path).splitlines():
                code = raw_line.split(";", 1)[0]
                definition = re.match(r"^([A-Za-z_][A-Za-z0-9_]*):", code)
                definition_label = definition.group(1) if definition else None
                for token in re.finditer(r"\b[A-Za-z_][A-Za-z0-9_]*\b", code):
                    label = token.group(0)
                    if label not in label_owners:
                        continue
                    if label == definition_label and token.start() == 0:
                        continue
                    occurrences[label][source_path] += 1

    owner_paths = {program["id"]: program["sourcePath"] for program in programs}
    label_rows = []
    program_counts = {program["id"]: Counter() for program in programs}
    for label, owner_program in sorted(label_owners.items()):
        owner_path = owner_paths[owner_program]
        same_file_count = occurrences[label][owner_path]
        external_sources = sorted(
            path for path, count in occurrences[label].items() if path != owner_path and count
        )
        external_count = sum(occurrences[label][path] for path in external_sources)
        program_counts[owner_program]["sameFileReferenceCount"] += same_file_count
        program_counts[owner_program]["externalReferenceCount"] += external_count
        if same_file_count or external_count:
            program_counts[owner_program]["referencedLabelCount"] += 1
        label_rows.append(
            {
                "label": label,
                "ownerProgram": owner_program,
                "sameFileReferenceCount": same_file_count,
                "externalReferenceCount": external_count,
                "externalSourcePaths": external_sources,
            }
        )

    program_rows = []
    for program in programs:
        counts = program_counts[program["id"]]
        reference_class = (
            "external"
            if counts["externalReferenceCount"]
            else "same-file-only"
            if counts["sameFileReferenceCount"]
            else "unreferenced"
        )
        program_rows.append(
            {
                "id": program["id"],
                "sourcePath": program["sourcePath"],
                "labelCount": len(program["labels"]),
                "referencedLabelCount": counts["referencedLabelCount"],
                "sameFileReferenceCount": counts["sameFileReferenceCount"],
                "externalReferenceCount": counts["externalReferenceCount"],
                "referenceClass": reference_class,
            }
        )

    class_counts = Counter(row["referenceClass"] for row in program_rows)
    return {
        "summary": {
            "scannedSourceFileCount": scanned_files,
            "referencedProgramCount": len(programs) - class_counts["unreferenced"],
            "externallyReferencedProgramCount": class_counts["external"],
            "sameFileOnlyProgramCount": class_counts["same-file-only"],
            "unreferencedProgramCount": class_counts["unreferenced"],
            "referencedLabelCount": sum(
                bool(row["sameFileReferenceCount"] or row["externalReferenceCount"])
                for row in label_rows
            ),
            "unreferencedLabelCount": sum(
                not row["sameFileReferenceCount"] and not row["externalReferenceCount"]
                for row in label_rows
            ),
            "sameFileReferenceCount": sum(
                row["sameFileReferenceCount"] for row in label_rows
            ),
            "externalReferenceCount": sum(
                row["externalReferenceCount"] for row in label_rows
            ),
        },
        "unreferencedPrograms": [
            row["id"] for row in program_rows if row["referenceClass"] == "unreferenced"
        ],
        "programs": program_rows,
        "labels": label_rows,
    }


def _enum_value(source: str, name: str) -> int:
    match = re.search(
        rf"^{re.escape(name)}:\s+equ\s+(\$?[0-9A-Fa-f]+)\b",
        source,
        re.MULTILINE,
    )
    if match is None:
        raise ValueError(f"map-script enum is missing: {name}")
    return _literal(match.group(1))


def _story_state_facts(disasm: Path, programs: list[dict[str, Any]]) -> dict[str, Any]:
    enums = read_upstream_text(disasm / "sf2enums.asm")
    yes_no_flag = _enum_value(enums, "FLAG_INDEX_YES_NO_PROMPT")
    battle_flag_start = _enum_value(enums, "BATTLE_UNLOCKED_FLAGS_START")
    reads = []
    writes = []
    prompt_writes = []
    battle_unlock_writes = []
    program_states = []
    read_counts: Counter[int] = Counter()
    write_counts: Counter[int] = Counter()
    for program in programs:
        program_reads: set[int] = set()
        program_sets: set[int] = set()
        program_clears: set[int] = set()
        program_prompts: set[int] = set()
        program_unlocks: set[int] = set()
        for command in program["commands"]:
            macro = command["macro"]
            if macro in {"jumpIfFlagSet", "jumpIfFlagClear"}:
                flag = _literal(command["arguments"][0])
                condition = "set" if macro == "jumpIfFlagSet" else "clear"
                read_counts[flag] += 1
                program_reads.add(flag)
                reads.append(
                    {
                        "program": program["id"],
                        "commandIndex": command["index"],
                        "flag": flag,
                        "condition": condition,
                        "targetSymbol": command["targetSymbol"],
                    }
                )
            elif macro in {"setF", "clearF", "csc10"}:
                flag = _literal(command["arguments"][0])
                operation = (
                    "set"
                    if macro == "setF"
                    else "clear"
                    if macro == "clearF"
                    else "set"
                    if _literal(command["arguments"][1]) != 0
                    else "clear"
                )
                write_counts[flag] += 1
                (program_sets if operation == "set" else program_clears).add(flag)
                writes.append(
                    {
                        "program": program["id"],
                        "commandIndex": command["index"],
                        "flag": flag,
                        "operation": operation,
                        "macro": macro,
                    }
                )
            elif macro == "yesNo":
                write_counts[yes_no_flag] += 1
                program_prompts.add(yes_no_flag)
                prompt_writes.append(
                    {
                        "program": program["id"],
                        "commandIndex": command["index"],
                        "flag": yes_no_flag,
                        "zeroResultOperation": "set",
                        "nonzeroResultOperation": "clear",
                    }
                )
            elif macro == "setStoryFlag":
                battle = _literal(command["arguments"][0])
                flag = battle_flag_start + battle
                write_counts[flag] += 1
                program_unlocks.add(flag)
                battle_unlock_writes.append(
                    {
                        "program": program["id"],
                        "commandIndex": command["index"],
                        "battle": battle,
                        "flag": flag,
                    }
                )
        if program_reads or program_sets or program_clears or program_prompts or program_unlocks:
            program_states.append(
                {
                    "program": program["id"],
                    "readFlags": sorted(program_reads),
                    "setFlags": sorted(program_sets),
                    "clearFlags": sorted(program_clears),
                    "promptFlags": sorted(program_prompts),
                    "battleUnlockFlags": sorted(program_unlocks),
                }
            )

    read_flags = set(read_counts)
    write_flags = set(write_counts)
    return {
        "summary": {
            "conditionalReadCount": len(reads),
            "uniqueReadFlagCount": len(read_flags),
            "directWriteCount": len(writes),
            "yesNoPromptWriteCount": len(prompt_writes),
            "battleUnlockWriteCount": len(battle_unlock_writes),
            "uniqueWriteFlagCount": len(write_flags),
            "readWriteOverlapCount": len(read_flags & write_flags),
            "statefulProgramCount": len(program_states),
        },
        "constants": {
            "yesNoPromptFlag": yes_no_flag,
            "battleUnlockedFlagsStart": battle_flag_start,
        },
        "readFlagCounts": {
            str(flag): count for flag, count in sorted(read_counts.items())
        },
        "writeFlagCounts": {
            str(flag): count for flag, count in sorted(write_counts.items())
        },
        "readWriteOverlapFlags": sorted(read_flags & write_flags),
        "directSetFlags": sorted(
            {row["flag"] for row in writes if row["operation"] == "set"}
        ),
        "directClearFlags": sorted(
            {row["flag"] for row in writes if row["operation"] == "clear"}
        ),
        "battleUnlockFlags": sorted({row["flag"] for row in battle_unlock_writes}),
        "conditionalReads": reads,
        "directWrites": writes,
        "yesNoPromptWrites": prompt_writes,
        "battleUnlockWrites": battle_unlock_writes,
        "programs": program_states,
    }


def _program_corpus(
    disasm: Path,
    source_paths: list[str],
    macro_contracts: dict[str, dict[str, Any]],
    addresses: dict[str, int],
) -> dict[str, Any]:
    target_ordinals = {
        "executeSubroutine": (0, "subroutine-call"),
        "jump": (0, "absolute-jump"),
        "jumpIfFlagSet": (1, "conditional-absolute-jump"),
        "jumpIfFlagClear": (1, "conditional-absolute-jump"),
        "jumpIfDefeatedByLastAttack": (1, "conditional-absolute-jump"),
        "jumpIfDead": (1, "conditional-absolute-jump"),
    }
    source_symbols = {
        match.group(1)
        for source_path in source_paths
        for _, line in _logical_source_lines(read_upstream_text(disasm / source_path))
        if line and not line[0].isspace()
        for match in [re.match(r"^([A-Za-z_][A-Za-z0-9_]*):", line)]
        if match
    }
    known_symbols = set(addresses) | source_symbols
    programs: list[dict[str, Any]] = []
    for source_path in source_paths:
        source = read_upstream_text(disasm / source_path)
        pending_labels: list[str] = []
        active: dict[str, Any] | None = None
        for line_number, logical_line in _logical_source_lines(source):
            statement = logical_line
            label_match = (
                re.match(r"^([A-Za-z_][A-Za-z0-9_]*):\s*(.*)$", logical_line)
                if logical_line and not logical_line[0].isspace()
                else None
            )
            if label_match:
                label, statement = label_match.groups()
                if active is None:
                    pending_labels.append(label)
                else:
                    active["labels"].append(label)
                if not statement:
                    continue
            invocation = _invocation(statement, macro_contracts)
            if invocation is None:
                if active is None:
                    pending_labels = []
                continue
            macro, arguments = invocation
            contract = macro_contracts[macro]
            if active is None:
                entry_label = pending_labels[0] if pending_labels else None
                entry = (
                    entry_label if entry_label else f"{source_path}#L{line_number}"
                )
                active = {
                    "id": entry,
                    "entryLabel": entry_label,
                    "address": addresses.get(entry),
                    "sourcePath": source_path,
                    "startLine": line_number,
                    "endLine": None,
                    "termination": None,
                    "labels": list(pending_labels),
                    "commands": [],
                }
                pending_labels = []
            command: dict[str, Any] = {
                "index": len(active["commands"]),
                "sourceLine": line_number,
                "macro": macro,
                "kind": contract["kind"],
                "opcode": contract["opcode"],
                "encodedBytes": contract["encodedBytes"],
                "arguments": arguments,
            }
            if macro in target_ordinals:
                ordinal, transfer_kind = target_ordinals[macro]
                if ordinal >= len(arguments):
                    raise ValueError(
                        f"map-script target argument is missing: {source_path}:{line_number}"
                    )
                target = _target_symbol(arguments[ordinal], known_symbols)
                command["transferKind"] = transfer_kind
                command["targetSymbol"] = target
                command["targetAddress"] = addresses.get(target)
            active["commands"].append(command)
            if macro == "csc_end":
                active["endLine"] = line_number
                active["termination"] = "csc-end"
                programs.append(active)
                active = None
                pending_labels = []
        if active is not None:
            if active["commands"][-1]["macro"] != "jump":
                raise ValueError(f"unterminated map-script program: {active['id']}")
            active["endLine"] = active["commands"][-1]["sourceLine"]
            active["termination"] = "absolute-jump"
            programs.append(active)

    if len({program["id"] for program in programs}) != len(programs):
        raise ValueError("duplicate map-script program entry label")
    label_owners: dict[str, str] = {}
    for program in programs:
        for label in program["labels"]:
            if label in label_owners:
                raise ValueError(f"duplicate map-script program label: {label}")
            label_owners[label] = program["id"]
    references = _program_references(disasm, label_owners, programs)
    story_state = _story_state_facts(disasm, programs)

    transfer_counts: Counter[str] = Counter()
    transfers = []
    for program in programs:
        for command in program["commands"]:
            if "transferKind" not in command:
                continue
            target_program = label_owners.get(command["targetSymbol"])
            relation = (
                "assembly-subroutine"
                if command["transferKind"] == "subroutine-call"
                and target_program is None
                else "same-program"
                if target_program == program["id"]
                else "cross-program"
                if target_program is not None
                else "unowned-script-target"
            )
            if relation == "unowned-script-target":
                raise ValueError(
                    f"map-script branch target has no program owner: {command['targetSymbol']}"
                )
            transfer_counts[f"{command['transferKind']}:{relation}"] += 1
            transfers.append(
                {
                    "sourceProgram": program["id"],
                    "commandIndex": command["index"],
                    "kind": command["transferKind"],
                    "targetSymbol": command["targetSymbol"],
                    "targetAddress": command["targetAddress"],
                    "targetProgram": target_program,
                    "relation": relation,
                }
            )

    command_count = sum(len(program["commands"]) for program in programs)
    source_only_programs = [
        {
            "id": program["id"],
            "sourcePath": program["sourcePath"],
            "termination": program["termination"],
        }
        for program in programs
        if program["address"] is None
    ]
    return {
        "summary": {
            "sourceFileCount": len(source_paths),
            "programCount": len(programs),
            "anonymousProgramCount": sum(
                program["entryLabel"] is None for program in programs
            ),
            "h1AddressedProgramCount": sum(
                program["address"] is not None for program in programs
            ),
            "sourceOnlyProgramCount": sum(
                program["address"] is None for program in programs
            ),
            "programLabelCount": len(label_owners),
            "cscEndTerminatedProgramCount": sum(
                program["termination"] == "csc-end" for program in programs
            ),
            "absoluteJumpTerminatedProgramCount": sum(
                program["termination"] == "absolute-jump" for program in programs
            ),
            "commandCount": command_count,
            "encodedCommandByteCount": sum(
                command["encodedBytes"]
                for program in programs
                for command in program["commands"]
            ),
            "transferCount": len(transfers),
            "sameProgramTransferCount": sum(
                transfer["relation"] == "same-program" for transfer in transfers
            ),
            "crossProgramTransferCount": sum(
                transfer["relation"] == "cross-program" for transfer in transfers
            ),
            "assemblySubroutineCallCount": sum(
                transfer["relation"] == "assembly-subroutine" for transfer in transfers
            ),
            "minimumCommandsPerProgram": min(
                len(program["commands"]) for program in programs
            ),
            "maximumCommandsPerProgram": max(
                len(program["commands"]) for program in programs
            ),
        },
        "transferCounts": dict(sorted(transfer_counts.items())),
        "referenceSummary": references["summary"],
        "storyState": story_state,
        "unreferencedPrograms": references["unreferencedPrograms"],
        "programReferences": references["programs"],
        "labelReferences": references["labels"],
        "sourceOnlyPrograms": source_only_programs,
        "largestPrograms": [
            {"id": program["id"], "commandCount": len(program["commands"])}
            for program in sorted(
                programs,
                key=lambda row: (-len(row["commands"]), row["id"]),
            )[:10]
        ],
        "labelOwners": dict(sorted(label_owners.items())),
        "transfers": transfers,
        "programs": programs,
    }


def _source_equates(disasm: Path) -> dict[str, int]:
    """Parse direct numeric source equates once for bounded command operands."""
    source = read_upstream_text(disasm / "sf2enums.asm")
    values: dict[str, int] = {}
    for match in re.finditer(
        r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s+equ\s+(?P<value>\$?[0-9A-Fa-f]+)\b",
        source,
        re.MULTILINE,
    ):
        values[match.group("name")] = _literal(match.group("value"))
    return values


def _dialogue_equates(equates: dict[str, int]) -> dict[str, int]:
    """Select the named source constants used by the dialogue command boundary."""
    missing = [name for name in DIALOGUE_CONSTANT_NAMES if name not in equates]
    if missing:
        raise ValueError(f"dialogue source constants are missing: {missing}")
    return equates


def _resolved_dialogue_operand(argument: str, constants: dict[str, int]) -> int:
    argument = argument.strip()
    if argument in constants:
        return constants[argument]
    try:
        return _literal(argument)
    except ValueError as error:
        raise ValueError(
            f"dialogue operand is not a literal or source constant: {argument}"
        ) from error


def _handler_by_name(handlers: list[dict[str, Any]], name: str) -> dict[str, Any]:
    rows = [row for row in handlers if row["name"] == name]
    if len(rows) != 1:
        raise ValueError(f"dialogue handler inventory is ambiguous: {name}")
    return rows[0]


def _next_statement(
    statements: list[str], start: int, pattern: str, *, owner: str
) -> tuple[int, re.Match[str]]:
    for index in range(start, len(statements)):
        match = re.fullmatch(pattern, statements[index])
        if match is not None:
            return index, match
    raise ValueError(f"{owner} statement is missing: {pattern}")


def _direct_call_sites(statements: list[str], target: str) -> list[int]:
    pattern = re.compile(
        rf"^(?:bsr|jsr)(?:\.[bwls])?\s+\(?{re.escape(target)}\)?(?:\.[bwls])?(?:\s|$)"
    )
    return [index for index, statement in enumerate(statements) if pattern.match(statement)]


def _stable_handler_statements(disasm: Path, handler: dict[str, Any]) -> list[str]:
    """Read one named handler section without relying on a file-wide fragment search."""
    source = read_upstream_text(disasm / handler["sourcePath"])
    match = re.search(
        rf"^{re.escape(handler['name'])}:\s*\n(?P<body>.*?)"
        rf"^\s*; End of function {re.escape(handler['name'])}\s*$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if match is None:
        raise ValueError(f"dialogue handler section is missing: {handler['name']}")
    statements = _statements(match.group("body"))
    if len(statements) != handler["statementCount"]:
        raise ValueError(f"dialogue handler statement inventory drift: {handler['name']}")
    return statements


def _signed_word(value: int) -> int:
    return value - 0x10000 if value & 0x8000 else value


def _dialogue_caller_breakdown(
    disasm: Path, handlers: list[dict[str, Any]], entity_dialogue_consumer: dict[str, Any]
) -> dict[str, Any]:
    """Inventory direct instruction targets and their resolved effective targets per handler."""
    caller_handlers = [
        _handler_by_name(handlers, name) for name in DIALOGUE_CALLER_HANDLER_NAMES
    ]
    bounded_source_paths = {handler["sourcePath"] for handler in caller_handlers}
    portrait_handler = _handler_by_name(handlers, PORTRAIT_HANDLER)
    if entity_dialogue_consumer["function"] != ENTITY_DIALOGUE_CONSUMER:
        raise ValueError("dialogue external consumer identity drift")
    target_resolutions = [
        {
            "instructionTarget": PORTRAIT_HANDLER,
            "effectiveTarget": portrait_handler["name"],
            "effectiveTargetSourcePath": portrait_handler["sourcePath"],
        },
        {
            "instructionTarget": ENTITY_DIALOGUE_CONSUMER,
            "effectiveTarget": entity_dialogue_consumer["function"],
            "effectiveTargetSourcePath": entity_dialogue_consumer["sourcePath"],
        },
    ]
    if [row["instructionTarget"] for row in target_resolutions] != list(DIALOGUE_CALLEE_TARGETS):
        raise ValueError("dialogue instruction target declaration drift")
    if len({row["effectiveTarget"] for row in target_resolutions}) != len(target_resolutions):
        raise ValueError("dialogue effective target declaration is ambiguous")
    for row in target_resolutions:
        row["effectiveTargetScope"] = (
            "internal"
            if row["effectiveTargetSourcePath"] in bounded_source_paths
            else "external"
        )

    effective_targets = [row["effectiveTarget"] for row in target_resolutions]
    caller_rows = []
    for handler in caller_handlers:
        statements = _stable_handler_statements(disasm, handler)
        instruction_counts = {
            target: len(_direct_call_sites(statements, target))
            for target in DIALOGUE_CALLEE_TARGETS
        }
        if any(count not in {0, 1} for count in instruction_counts.values()):
            raise ValueError(f"dialogue caller site count drift: {handler['name']}")
        effective_counts = {target: 0 for target in effective_targets}
        for resolution in target_resolutions:
            effective_counts[resolution["effectiveTarget"]] += instruction_counts[
                resolution["instructionTarget"]
            ]
        caller_rows.append(
            {
                "handler": handler["name"],
                "sourcePath": handler["sourcePath"],
                "instructionTargetSiteCounts": instruction_counts,
                "effectiveTargetSiteCounts": effective_counts,
            }
        )

    instruction_totals = {
        target: sum(row["instructionTargetSiteCounts"][target] for row in caller_rows)
        for target in DIALOGUE_CALLEE_TARGETS
    }
    effective_totals = {
        target: sum(row["effectiveTargetSiteCounts"][target] for row in caller_rows)
        for target in effective_targets
    }

    def scoped_totals(counts: dict[str, int], *, target_field: str, scope: str) -> dict[str, int]:
        scopes = {
            row[target_field]: row["effectiveTargetScope"] for row in target_resolutions
        }
        return {
            target: counts[target] if scopes[target] == scope else 0 for target in counts
        }

    return {
        "callerHandlers": caller_rows,
        "targetResolutions": target_resolutions,
        "instructionTargetTotals": instruction_totals,
        "effectiveTargetTotals": effective_totals,
        "internalInstructionTargetTotals": scoped_totals(
            instruction_totals, target_field="instructionTarget", scope="internal"
        ),
        "externalInstructionTargetTotals": scoped_totals(
            instruction_totals, target_field="instructionTarget", scope="external"
        ),
        "internalEffectiveTargetTotals": scoped_totals(
            effective_totals, target_field="effectiveTarget", scope="internal"
        ),
        "externalEffectiveTargetTotals": scoped_totals(
            effective_totals, target_field="effectiveTarget", scope="external"
        ),
    }


def _dialogue_handler_facts(
    disasm: Path,
    macros: dict[str, dict[str, Any]],
    dispatch_targets: list[str],
    handlers: list[dict[str, Any]],
    modifier_entity_byte_pairs: Counter[tuple[int, int]],
    entity_dialogue_consumer: dict[str, Any],
) -> tuple[list[dict[str, Any]], dict[str, Any], dict[str, Any]]:
    """Guard the named dialogue handlers from their smallest stable sections."""
    handler_facts = []
    sentinel_values = []
    for macro in DIALOGUE_MACROS:
        contract = macros[macro]
        if contract["kind"] != "command" or contract["aliasOf"] is not None:
            raise ValueError(f"dialogue macro is not a primary command: {macro}")
        opcode = contract["opcode"]
        if opcode is None or dispatch_targets[opcode] != DIALOGUE_HANDLER_BY_MACRO[macro]:
            raise ValueError(f"dialogue macro dispatcher target drift: {macro}")
        handler = _handler_by_name(handlers, DIALOGUE_HANDLER_BY_MACRO[macro])
        if handler["opcodes"] != [opcode]:
            raise ValueError(f"dialogue handler opcode inventory drift: {handler['name']}")
        if handler["encodedCommandBytes"] != contract["encodedBytes"]:
            raise ValueError(f"dialogue handler encoded-width drift: {handler['name']}")
        statements = _stable_handler_statements(disasm, handler)
        if macro in DIALOGUE_DISPLAY_MACROS:
            skip_index = next(
                (
                    index
                    for index, statement in enumerate(statements)
                    if statement == "tst.b ((SKIP_CUTSCENE_TEXT-$1000000)).w"
                ),
                None,
            )
            skip_guard = None
            expects_skip_guard = macro in {"nextSingleText", "nextText"}
            if (skip_index is not None) != expects_skip_guard:
                raise ValueError(f"dialogue skip-guard admission drift: {handler['name']}")
            if skip_index is not None:
                branch_index, branch = _next_statement(
                    statements,
                    skip_index + 1,
                    r"bne\.[bwls]\s+\S+",
                    owner=handler["name"],
                )
                if branch_index != skip_index + 1:
                    raise ValueError(f"dialogue skip-guard order drift: {handler['name']}")
                skip_guard = {
                    "predicate": statements[skip_index],
                    "branch": branch.group(0),
                }

            sentinel_index, sentinel = _next_statement(
                statements,
                0,
                r"cmpi\.w\s+#(?P<value>-?\$?[0-9A-Fa-f]+),\(a6\)",
                owner=handler["name"],
            )
            branch_index, branch = _next_statement(
                statements,
                sentinel_index + 1,
                r"beq\.[bwls]\s+\S+",
                owner=handler["name"],
            )
            if branch_index != sentinel_index + 1:
                raise ValueError(f"dialogue sentinel-branch order drift: {handler['name']}")
            sentinel_value = _literal(sentinel.group("value")) & 0xFFFF
            sentinel_values.append(sentinel_value)
            portrait_sites = _direct_call_sites(statements, PORTRAIT_HANDLER)
            consumer_sites = _direct_call_sites(statements, ENTITY_DIALOGUE_CONSUMER)
            if len(portrait_sites) != 1 or len(consumer_sites) != 1:
                raise ValueError(f"dialogue helper call count drift: {handler['name']}")
            if portrait_sites[0] >= consumer_sites[0]:
                raise ValueError(f"dialogue helper call order drift: {handler['name']}")
            increment_index, _ = _next_statement(
                statements,
                consumer_sites[0] + 1,
                r"addq\.w\s+#1,\(\(CUTSCENE_DIALOG_INDEX-\$1000000\)\)\.w",
                owner=handler["name"],
            )
            display_index, _ = _next_statement(
                statements,
                consumer_sites[0] + 1,
                r"jsr\s+\(DisplayText\)\.l",
                owner=handler["name"],
            )
            if display_index >= increment_index:
                raise ValueError(f"dialogue display/index increment order drift: {handler['name']}")
            name_index_statements = [
                statement
                for statement in statements
                if statement
                in {
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_2-$1000000)).w",
                }
            ]
            if macro.endswith("Var"):
                if name_index_statements != [
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_2-$1000000)).w",
                ]:
                    raise ValueError(
                        f"dialogue variable name-word consumption drift: {handler['name']}"
                    )
            elif name_index_statements:
                raise ValueError(f"dialogue fixed handler consumes name words: {handler['name']}")
            is_single = macro.startswith("nextSingle")
            close_sequence = [
                "jsr j_ClosePortraitWindow",
                "clsTxt",
                "moveq #10,d0",
                "jsr (Sleep).w",
            ]
            if is_single:
                cursor = increment_index + 1
                for statement in close_sequence:
                    cursor, _ = _next_statement(
                        statements, cursor, re.escape(statement), owner=handler["name"]
                    )
                    cursor += 1
            elif any(statement in statements for statement in close_sequence):
                raise ValueError(f"dialogue continuing close/sleep shape drift: {handler['name']}")
            handler_facts.append(
                {
                    "macro": macro,
                    "handler": handler["name"],
                    "address": handler["address"],
                    "opcode": opcode,
                    "skipGuard": skip_guard,
                    "modifierEntityWordSentinel": {
                        "unsignedValue": sentinel_value,
                        "signedValue": _signed_word(sentinel_value),
                        "branch": branch.group(0),
                    },
                    "nameWordDestinationCount": len(name_index_statements),
                    "displayThenIndexIncrement": True,
                    "singleCloseSleepSequence": is_single,
                }
            )
        elif macro == "textCursor":
            cursor_index, _ = _next_statement(
                statements,
                0,
                r"move\.w\s+\(a6\)\+,\(\(CUTSCENE_DIALOG_INDEX-\$1000000\)\)\.w",
                owner=handler["name"],
            )
            if cursor_index != 0:
                raise ValueError("dialogue text-index write is not the first handler statement")
            handler_facts.append(
                {
                    "macro": macro,
                    "handler": handler["name"],
                    "address": handler["address"],
                    "opcode": opcode,
                    "cursorWrite": statements[cursor_index],
                }
            )
        else:
            close_index, _ = _next_statement(
                statements,
                0,
                r"jsr\s+j_ClosePortraitWindow",
                owner=handler["name"],
            )
            clear_index, _ = _next_statement(
                statements, close_index + 1, r"clsTxt", owner=handler["name"]
            )
            if (close_index, clear_index) != (0, 1):
                raise ValueError("dialogue hide-window call order drift")
            handler_facts.append(
                {
                    "macro": macro,
                    "handler": handler["name"],
                    "address": handler["address"],
                    "opcode": opcode,
                    "closeThenClear": True,
                }
            )

    if len(set(sentinel_values)) != 1:
        raise ValueError("dialogue handlers disagree on modifier/entity word sentinel")
    sentinel_value = sentinel_values[0]

    portrait_handler = _handler_by_name(handlers, PORTRAIT_HANDLER)
    portrait_statements = _stable_handler_statements(disasm, portrait_handler)
    word_read_index, _ = _next_statement(
        portrait_statements,
        0,
        r"move\.w\s+\(a6\)\+,d0",
        owner=PORTRAIT_HANDLER,
    )
    bit_rows = []
    cursor = word_read_index + 1
    for destination in ("d3", "d4"):
        zero_index, _ = _next_statement(
            portrait_statements,
            cursor,
            rf"moveq\s+#0,{destination}",
            owner=PORTRAIT_HANDLER,
        )
        bit_index, bit_match = _next_statement(
            portrait_statements,
            zero_index + 1,
            r"btst\s+#(?P<bit>\$?[0-9A-Fa-f]+),d0",
            owner=PORTRAIT_HANDLER,
        )
        branch_index, _ = _next_statement(
            portrait_statements,
            bit_index + 1,
            r"beq\.[bwls]\s+\S+",
            owner=PORTRAIT_HANDLER,
        )
        set_index, _ = _next_statement(
            portrait_statements,
            branch_index + 1,
            rf"moveq\s+#-1,{destination}",
            owner=PORTRAIT_HANDLER,
        )
        if not (zero_index < bit_index < branch_index < set_index):
            raise ValueError(f"portrait modifier branch order drift: {destination}")
        bit_rows.append({"bit": _literal(bit_match.group("bit")), "destination": destination})
        cursor = set_index + 1
    handler_tested_modifier_byte_mask = 0
    for row in bit_rows:
        byte_bit = row["bit"] - 8
        if not 0 <= byte_bit <= 7:
            raise ValueError("portrait modifier bit is outside the packed modifier byte")
        handler_tested_modifier_byte_mask |= 1 << byte_bit
    if len({row["bit"] for row in bit_rows}) != len(bit_rows):
        raise ValueError("portrait modifier bit test is duplicated")
    full_word_sentinel_bytes = (sentinel_value >> 8, sentinel_value & 0xFF)
    for modifier, entity in modifier_entity_byte_pairs:
        if (modifier, entity) == full_word_sentinel_bytes:
            continue
        if modifier & ~handler_tested_modifier_byte_mask:
            raise ValueError(
                "dialogue non-sentinel modifier byte exceeds handler-tested modifier byte mask"
            )
    consumer_sites = _direct_call_sites(portrait_statements, ENTITY_DIALOGUE_CONSUMER)
    if len(consumer_sites) != 1:
        raise ValueError("portrait helper consumer call count drift")
    if consumer_sites[0] < cursor:
        raise ValueError("portrait helper consumer call order drift")
    return (
        handler_facts,
        {
            "handler": PORTRAIT_HANDLER,
            "address": portrait_handler["address"],
            "sourcePath": portrait_handler["sourcePath"],
            "modifierEntityWordRead": portrait_statements[word_read_index],
            "handlerTestedModifierByteMask": handler_tested_modifier_byte_mask,
            "modifierBitTests": bit_rows,
        },
        _dialogue_caller_breakdown(disasm, handlers, entity_dialogue_consumer),
    )


def _entity_dialogue_consumer_facts(
    disasm: Path, constants: dict[str, int], addresses: dict[str, int]
) -> dict[str, Any]:
    source = read_upstream_text(disasm / ENTITY_DIALOGUE_CONSUMER_PATH)
    function = re.search(
        rf"^{ENTITY_DIALOGUE_CONSUMER}:\s*\n(?P<body>.*?)"
        rf"^\s*; End of function {ENTITY_DIALOGUE_CONSUMER}\s*$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if function is None:
        raise ValueError("entity dialogue consumer function is missing")
    statements = _statements(function.group("body"))
    mask_index, mask = _next_statement(
        statements,
        0,
        r"andi\.w\s+#(?P<name>[A-Za-z_][A-Za-z0-9_]*),d0",
        owner=ENTITY_DIALOGUE_CONSUMER,
    )
    constant_name = mask.group("name")
    if constant_name != "COMBATANT_MASK_ALL" or constant_name not in constants:
        raise ValueError("entity dialogue consumer low-domain mask drift")
    entity_call_index, _ = _next_statement(
        statements,
        mask_index + 1,
        r"bsr\.w\s+GetEntityAddressFromCharacter",
        owner=ENTITY_DIALOGUE_CONSUMER,
    )
    map_sprite_index, _ = _next_statement(
        statements,
        entity_call_index + 1,
        r"move\.b\s+ENTITYDEF_OFFSET_MAPSPRITE\(a5\),d0",
        owner=ENTITY_DIALOGUE_CONSUMER,
    )
    if not (mask_index < entity_call_index < map_sprite_index):
        raise ValueError("entity dialogue consumer low-domain order drift")
    if ENTITY_DIALOGUE_CONSUMER not in addresses:
        raise ValueError("entity dialogue consumer H1 address is missing")
    return {
        "function": ENTITY_DIALOGUE_CONSUMER,
        "address": addresses[ENTITY_DIALOGUE_CONSUMER],
        "sourcePath": ENTITY_DIALOGUE_CONSUMER_PATH.as_posix(),
        "lowDomainMask": {"constant": constant_name, "value": constants[constant_name]},
        "mapSpriteLoad": statements[map_sprite_index],
    }


def _modifier_source_labels(
    disasm: Path, modifier_bit_tests: list[dict[str, Any]], sentinel_value: int
) -> list[dict[str, Any]]:
    """Retain the macro's original modifier labels without treating them as new semantics."""
    blocks = _macro_blocks(read_upstream_text(disasm / MACRO_PATH))
    labels_by_macro: list[dict[int, str]] = []
    for macro in DIALOGUE_MODIFIER_MACROS:
        body = blocks.get(macro)
        if body is None:
            raise ValueError(f"dialogue modifier macro is missing: {macro}")
        match = re.search(r"dc\.b\s+\\1\s*;\s*portrait modifier \(([^)]+)\)", body)
        if match is None:
            raise ValueError(f"dialogue modifier labels are missing: {macro}")
        labels = {}
        for entry in match.group(1).split(","):
            value, label = entry.strip().split("-", 1)
            labels[_literal(value)] = label
        labels_by_macro.append(labels)
    if labels_by_macro[0] != labels_by_macro[1]:
        raise ValueError("dialogue modifier labels disagree between source macros")
    labels = labels_by_macro[0]
    full_word_sentinel_high_byte = sentinel_value >> 8
    expected_by_bit = {bit - 8: bit for bit in (row["bit"] for row in modifier_bit_tests)}
    result = []
    for value, label in sorted(labels.items()):
        row = {
            "modifierByteValue": value,
            "sourceLabel": label,
            "handlerWordBit": None,
        }
        if value not in {0, full_word_sentinel_high_byte}:
            if value <= 0 or value & (value - 1) or value.bit_length() - 1 not in expected_by_bit:
                raise ValueError("dialogue modifier label no longer matches a handler bit test")
            row["handlerWordBit"] = expected_by_bit[value.bit_length() - 1]
        result.append(row)
    if full_word_sentinel_high_byte not in labels or 0 not in labels:
        raise ValueError("dialogue modifier label boundary drift")
    return result


def _dialogue_command_facts(
    disasm: Path,
    source_equates: dict[str, int],
    macros: dict[str, dict[str, Any]],
    dispatch_targets: list[str],
    handlers: list[dict[str, Any]],
    program_corpus: dict[str, Any],
    addresses: dict[str, int],
    rom_path: Path,
    upstream_path: Path,
) -> dict[str, Any]:
    """Build the dialogue command contract from program references and source use sites."""
    constants = _dialogue_equates(source_equates)
    programs = program_corpus["programs"]
    source_references = []
    program_totals = []
    source_counts: Counter[str] = Counter()
    modifier_values: Counter[int] = Counter()
    modifier_entity_byte_pairs: Counter[tuple[int, int]] = Counter()
    entity_values: Counter[int] = Counter()
    text_cursor_values: Counter[int] = Counter()
    for program in programs:
        counts: Counter[str] = Counter()
        command_indexes = []
        for command in program["commands"]:
            macro = command["macro"]
            if macro not in DIALOGUE_MACROS:
                continue
            command_indexes.append(command["index"])
            source_counts[macro] += 1
            counts[macro] += 1
            arguments = command["arguments"]
            if macro in DIALOGUE_DISPLAY_MACROS:
                if len(arguments) < 2:
                    raise ValueError(f"dialogue display command lacks modifier/entity: {macro}")
                modifier = _resolved_dialogue_operand(arguments[0], constants)
                entity = _resolved_dialogue_operand(arguments[1], constants)
                if not 0 <= modifier <= 0xFF or not 0 <= entity <= 0xFF:
                    raise ValueError(f"dialogue modifier/entity byte domain drift: {macro}")
                modifier_values[modifier] += 1
                modifier_entity_byte_pairs[(modifier, entity)] += 1
                entity_values[entity] += 1
            elif macro == "textCursor":
                if len(arguments) != 1:
                    raise ValueError("dialogue text cursor operand count drift")
                text_cursor_values[_resolved_dialogue_operand(arguments[0], constants)] += 1
            elif arguments:
                raise ValueError("dialogue hide command unexpectedly has operands")
        program_totals.append(
            {
                "programId": program["id"],
                "commandCount": sum(counts.values()),
                "macroCounts": {name: counts[name] for name in DIALOGUE_MACROS},
            }
        )
        if command_indexes:
            source_references.append(
                {"programId": program["id"], "commandIndexes": command_indexes}
            )
    if len(program_totals) != program_corpus["summary"]["programCount"]:
        raise ValueError("dialogue zero-inclusive program total coverage drift")
    if sum(len(row["commandIndexes"]) for row in source_references) != sum(source_counts.values()):
        raise ValueError("dialogue source reference count drift")
    flattened_references = [
        (row["programId"], command_index)
        for row in source_references
        for command_index in row["commandIndexes"]
    ]
    if len(set(flattened_references)) != len(flattened_references):
        raise ValueError("dialogue source reference identity drift")
    for name in DIALOGUE_MACROS:
        if sum(row["macroCounts"][name] for row in program_totals) != source_counts[name]:
            raise ValueError(f"dialogue per-program total drift: {name}")

    text_line_domain = build_text_line_domain_contract(rom_path, upstream_path)
    domain = text_line_domain["gamescriptFacts"]
    if not text_cursor_values:
        raise ValueError("dialogue text-cursor source use is absent")
    if (
        min(text_cursor_values) < domain["firstLineId"]
        or max(text_cursor_values) > domain["lastLineId"]
    ):
        raise ValueError("dialogue text-cursor value is outside the source text-line domain")

    sprite_dialogue = build_sprite_dialogue_contract(rom_path, upstream_path)
    if (
        sprite_dialogue["upstream"]["commit"] != text_line_domain["upstream"]["commit"]
        or sprite_dialogue["romSha256"] != text_line_domain["romSha256"]
        or sprite_dialogue["summary"]["rowCount"] != 119
    ):
        raise ValueError("dialogue sprite-property contract provenance or row boundary drift")

    entity_dialogue_consumer = _entity_dialogue_consumer_facts(disasm, constants, addresses)
    handler_facts, portrait_helper, caller_breakdown = _dialogue_handler_facts(
        disasm,
        macros,
        dispatch_targets,
        handlers,
        modifier_entity_byte_pairs,
        entity_dialogue_consumer,
    )
    modifier_source_labels = _modifier_source_labels(
        disasm,
        portrait_helper["modifierBitTests"],
        handler_facts[0]["modifierEntityWordSentinel"]["unsignedValue"],
    )
    selected_macros = []
    for name in DIALOGUE_MACROS:
        contract = macros[name]
        selected_macros.append(
            {
                "name": name,
                "opcode": contract["opcode"],
                "encodedBytes": contract["encodedBytes"],
                "operandBytes": contract["operandBytes"],
                "operandLayout": contract["operandLayout"],
                "parameterOrdinals": contract["parameterOrdinals"],
                "handler": DIALOGUE_HANDLER_BY_MACRO[name],
                "handlerAddress": _handler_by_name(
                    handlers, DIALOGUE_HANDLER_BY_MACRO[name]
                )["address"],
                "sourceCommandCount": source_counts[name],
            }
        )
    return {
        "macros": selected_macros,
        "sourceSiteReferences": source_references,
        "programTotals": program_totals,
        "operandFacts": {
            "constants": {name: constants[name] for name in DIALOGUE_CONSTANT_NAMES},
            "modifierByteCounts": [
                {"value": value, "count": modifier_values[value]}
                for value in sorted(modifier_values)
            ],
            "modifierSourceLabels": modifier_source_labels,
            "entityByteCounts": [
                {"value": value, "count": entity_values[value]} for value in sorted(entity_values)
            ],
            "textCursorValueCounts": [
                {"value": value, "count": text_cursor_values[value]}
                for value in sorted(text_cursor_values)
            ],
            "textCursorValueBounds": {
                "minimum": min(text_cursor_values),
                "maximum": max(text_cursor_values),
                "domainMinimum": domain["firstLineId"],
                "domainMaximum": domain["lastLineId"],
            },
        },
        "handlers": handler_facts,
        "portraitHelper": portrait_helper,
        "callerBreakdown": caller_breakdown,
        "entityDialogueConsumer": entity_dialogue_consumer,
        "textLineDomain": {
            "contractId": text_line_domain["id"],
            "upstreamCommit": text_line_domain["upstream"]["commit"],
            "romSha256": text_line_domain["romSha256"],
            "sourcePath": domain["sourcePath"],
            "lineIdCount": domain["lineIdCount"],
            "firstLineId": domain["firstLineId"],
            "lastLineId": domain["lastLineId"],
            "idsAreContiguous": domain["idsAreContiguous"],
        },
        "spriteDialogueTable": {
            "contractId": sprite_dialogue["id"],
            "upstreamCommit": sprite_dialogue["upstream"]["commit"],
            "romSha256": sprite_dialogue["romSha256"],
            "tableAddress": sprite_dialogue["table"]["table_MapspriteDialogueProperties"],
            "consumerAddress": sprite_dialogue["table"][ENTITY_DIALOGUE_CONSUMER],
            "rowCount": sprite_dialogue["summary"]["rowCount"],
            "recordByteCount": sprite_dialogue["summary"]["recordByteCount"],
            "tableByteCount": sprite_dialogue["summary"]["tableByteCount"],
            "sourcePath": sprite_dialogue["romRange"]["sourcePath"],
        },
        "runtimeQuestions": DIALOGUE_RUNTIME_QUESTIONS,
    }


def _transition_command_facts(
    disasm: Path,
    equates: dict[str, int],
    macros: dict[str, dict[str, Any]],
    dispatch_targets: list[str],
    handlers: list[dict[str, Any]],
    program_corpus: dict[str, Any],
    rom_path: Path,
    upstream_path: Path,
) -> dict[str, Any]:
    """Build the bounded map transition command contract from source operands and handlers."""
    required_constants = ("MAP_EVENT_WARP", "MAP_CURRENT", "RIGHT", "UP", "LEFT", "DOWN")
    missing = [name for name in required_constants if name not in equates]
    if missing:
        raise ValueError(f"transition source constants are missing: {missing}")
    map_content = build_map_content_contract(rom_path, upstream_path)
    canonical_map_ids = {row["map"] for row in map_content["mapEntries"]}
    if len(canonical_map_ids) != map_content["summary"]["mapCount"]:
        raise ValueError("transition canonical map identity drift")
    facing_values = {equates[name] for name in ("RIGHT", "UP", "LEFT", "DOWN")}
    source_counts: Counter[str] = Counter()
    sites = []
    program_totals = []
    for program in program_corpus["programs"]:
        command_rows = []
        counts: Counter[str] = Counter()
        for command in program["commands"]:
            macro = command["macro"]
            if macro not in TRANSITION_MACROS:
                continue
            arguments = command["arguments"]
            map_argument = None
            map_value = None
            map_domain = None
            coordinate_x = None
            coordinate_y = None
            facing_value = None
            if macro == "warp":
                if len(arguments) != 4:
                    raise ValueError("transition warp operand count drift")
                map_argument = arguments[0]
                map_value = _resolved_dialogue_operand(map_argument, equates)
                coordinate_x = _resolved_dialogue_operand(arguments[1], equates)
                coordinate_y = _resolved_dialogue_operand(arguments[2], equates)
                facing_value = _resolved_dialogue_operand(arguments[3], equates)
                if not all(0 <= value <= 0xFF for value in (map_value, coordinate_x, coordinate_y)):
                    raise ValueError("transition warp byte operand domain drift")
                if facing_value not in facing_values:
                    raise ValueError("transition warp facing source domain drift")
            elif macro in {"loadMapFadeIn", "mapLoad"}:
                if len(arguments) != 3:
                    raise ValueError(f"transition map-load operand count drift: {macro}")
                map_argument = arguments[0]
                map_value = _resolved_dialogue_operand(map_argument, equates)
                coordinate_x = _resolved_dialogue_operand(arguments[1], equates)
                coordinate_y = _resolved_dialogue_operand(arguments[2], equates)
            elif macro == "reloadMap":
                if len(arguments) != 2:
                    raise ValueError("transition reload operand count drift")
                coordinate_x = _resolved_dialogue_operand(arguments[0], equates)
                coordinate_y = _resolved_dialogue_operand(arguments[1], equates)
            elif arguments:
                raise ValueError("transition reset command unexpectedly has operands")
            if map_value is not None:
                if map_value in canonical_map_ids:
                    map_domain = "canonical-map"
                elif map_value == equates["MAP_CURRENT"] and map_argument == "MAP_CURRENT":
                    map_domain = "source-map-current"
                else:
                    raise ValueError(
                        "transition destination map is outside the canonical map domain"
                    )
            if any(
                value is not None and not 0 <= value <= 0xFFFF
                for value in (coordinate_x, coordinate_y)
            ):
                raise ValueError("transition coordinate word domain drift")
            counts[macro] += 1
            source_counts[macro] += 1
            command_rows.append(
                {
                    "commandIndex": command["index"],
                    "macro": macro,
                    "destinationMapOperand": map_argument,
                    "destinationMapValue": map_value,
                    "destinationMapDomain": map_domain,
                    "coordinateXValue": coordinate_x,
                    "coordinateYValue": coordinate_y,
                    "facingValue": facing_value,
                }
            )
        program_totals.append(
            {
                "programId": program["id"],
                "commandCount": sum(counts.values()),
                "macroCounts": {name: counts[name] for name in TRANSITION_MACROS},
            }
        )
        if command_rows:
            sites.append({"programId": program["id"], "commands": command_rows})
    if len(program_totals) != program_corpus["summary"]["programCount"]:
        raise ValueError("transition zero-inclusive program domain drift")
    if sum(len(row["commands"]) for row in sites) != sum(source_counts.values()):
        raise ValueError("transition source-site coverage drift")
    for macro in TRANSITION_MACROS:
        if sum(row["macroCounts"][macro] for row in program_totals) != source_counts[macro]:
            raise ValueError(f"transition program total drift: {macro}")

    handler_rows = []
    handler_by_macro = {}
    for macro in TRANSITION_MACROS:
        contract = macros[macro]
        opcode = contract["opcode"]
        handler_name = TRANSITION_HANDLER_BY_MACRO[macro]
        if contract["kind"] != "command" or contract["aliasOf"] is not None:
            raise ValueError(f"transition macro is not primary: {macro}")
        if opcode is None or dispatch_targets[opcode] != handler_name:
            raise ValueError(f"transition dispatcher target drift: {macro}")
        handler = _handler_by_name(handlers, handler_name)
        if (
            handler["opcodes"] != [opcode]
            or handler["encodedCommandBytes"] != contract["encodedBytes"]
        ):
            raise ValueError(f"transition handler ABI drift: {handler_name}")
        handler_by_macro[macro] = handler

    warp = _stable_handler_statements(disasm, handler_by_macro["warp"])
    event_write = (
        re.fullmatch(r"move\.w\s+#(?P<value>\$?[0-9A-Fa-f]+),\(a0\)\+", warp[1])
        if len(warp) > 1
        else None
    )
    clear_write = (
        re.fullmatch(r"move\.b\s+#(?P<value>\$?[0-9A-Fa-f]+),\(a0\)\+", warp[2])
        if len(warp) > 2
        else None
    )
    warp_byte_reads = len(macros["warp"]["operandLayout"])
    if (
        len(warp) != warp_byte_reads + 4
        or warp[0] != "lea ((MAP_EVENT_TYPE-$1000000)).w,a0"
        or event_write is None
        or _literal(event_write.group("value")) != equates["MAP_EVENT_WARP"]
        or clear_write is None
        or warp[3:-1] != ["move.b (a6)+,(a0)+"] * warp_byte_reads
        or warp[-1] != "rts"
    ):
        raise ValueError("transition warp state/cursor sequence drift")
    map_event_clear_byte_value = _literal(clear_write.group("value"))
    handler_rows.append(
        {
            "macro": "warp",
            "handler": handler_by_macro["warp"]["name"],
            "address": handler_by_macro["warp"]["address"],
            "opcode": macros["warp"]["opcode"],
            "cursorReadWidths": [
                field["widthBytes"] for field in macros["warp"]["operandLayout"]
            ],
            "mapEventTypeValue": equates["MAP_EVENT_WARP"],
            "mapEventClearByteValue": map_event_clear_byte_value,
            "d1Immediate": None,
            "packedCoordinateMultiplier": None,
            "directServiceCalls": [],
            "fallsThroughTo": None,
        }
    )
    reset = _stable_handler_statements(disasm, handler_by_macro["resetMap"])
    reset_expected = ["move.l a6,-(sp)", "jsr (ResetCurrentMap).l", "movea.l (sp)+,a6", "rts"]
    if reset != reset_expected:
        raise ValueError("transition reset service/cursor preservation drift")
    load_fade = _stable_handler_statements(disasm, handler_by_macro["loadMapFadeIn"])
    fade_source = read_upstream_text(disasm / handler_by_macro["loadMapFadeIn"]["sourcePath"])
    fade_match = re.search(r"^csc37_loadMapAndFadeIn:\s*$", fade_source, re.MULTILINE)
    load_match = re.search(r"^csc48_loadMap:\s*$", fade_source, re.MULTILINE)
    if fade_match is None or load_match is None or fade_match.start() >= load_match.start():
        raise ValueError("transition fade-load section boundary drift")
    between_fade_and_load = fade_source[fade_match.end() : load_match.start()]
    if any(statement == "rts" for statement in load_fade) or re.search(
        r"^[A-Za-z_][A-Za-z0-9_]*:\s*$", between_fade_and_load, re.MULTILINE
    ):
        raise ValueError("transition fade-load fallthrough drift")
    load = _stable_handler_statements(disasm, handler_by_macro["mapLoad"])
    load_map_probe, _ = _next_statement(load, 0, r"move\.w\s+\(a6\),d1", owner="csc48_loadMap")
    tileset_call, _ = _next_statement(
        load, load_map_probe + 1, r"jsr\s+\(LoadMapTilesets\)\.w", owner="csc48_loadMap"
    )
    map_read, _ = _next_statement(
        load, tileset_call + 1, r"move\.w\s+\(a6\)\+,d1", owner="csc48_loadMap"
    )
    x_read, _ = _next_statement(load, map_read + 1, r"move\.w\s+\(a6\)\+,d0", owner="csc48_loadMap")
    y_read, _ = _next_statement(load, x_read + 1, r"move\.w\s+\(a6\)\+,d2", owner="csc48_loadMap")
    scale, load_scale_match = _next_statement(
        load,
        y_read + 1,
        r"mulu\.w\s+#(?P<scale>\$?[0-9A-Fa-f]+),d0",
        owner="csc48_loadMap",
    )
    load_scale_value = _literal(load_scale_match.group("scale"))
    map_call, _ = _next_statement(load, scale + 1, r"jsr\s+\(LoadMap\)\.w", owner="csc48_loadMap")
    enable_call, _ = _next_statement(
        load, map_call + 1, r"jsr\s+\(EnableDisplayAndInterrupts\)\.w", owner="csc48_loadMap"
    )
    if (
        not load_map_probe
        < tileset_call
        < map_read
        < x_read
        < y_read
        < scale
        < map_call
        < enable_call
    ):
        raise ValueError("transition map-load cursor/service order drift")
    reload = _stable_handler_statements(disasm, handler_by_macro["reloadMap"])
    selector, selector_match = _next_statement(
        reload,
        0,
        r"moveq\s+#(?P<selector>-?\$?[0-9A-Fa-f]+),d1",
        owner="csc46_reloadMap",
    )
    reload_selector_value = _literal(selector_match.group("selector"))
    reload_x, _ = _next_statement(
        reload, selector + 1, r"move\.w\s+\(a6\)\+,d0", owner="csc46_reloadMap"
    )
    reload_y, _ = _next_statement(
        reload, reload_x + 1, r"move\.w\s+\(a6\)\+,d2", owner="csc46_reloadMap"
    )
    reload_scale, reload_scale_match = _next_statement(
        reload,
        reload_y + 1,
        r"mulu\.w\s+#(?P<scale>\$?[0-9A-Fa-f]+),d0",
        owner="csc46_reloadMap",
    )
    reload_scale_value = _literal(reload_scale_match.group("scale"))
    reload_call, _ = _next_statement(
        reload, reload_scale + 1, r"jsr\s+\(LoadMap\)\.w", owner="csc46_reloadMap"
    )
    reload_enable, _ = _next_statement(
        reload,
        reload_call + 1,
        r"jsr\s+\(EnableDisplayAndInterrupts\)\.w",
        owner="csc46_reloadMap",
    )
    if not selector < reload_x < reload_y < reload_scale < reload_call < reload_enable:
        raise ValueError("transition reload cursor/service order drift")
    if load_scale_value != reload_scale_value:
        raise ValueError("transition coordinate selector scale disagreement")
    handler_rows.extend(
        [
            {
                "macro": "resetMap",
                "handler": handler_by_macro["resetMap"]["name"],
                "address": handler_by_macro["resetMap"]["address"],
                "opcode": macros["resetMap"]["opcode"],
                "cursorReadWidths": [
                    field["widthBytes"] for field in macros["resetMap"]["operandLayout"]
                ],
                "mapEventTypeValue": None,
                "mapEventClearByteValue": None,
                "d1Immediate": None,
                "packedCoordinateMultiplier": None,
                "directServiceCalls": [],
                "fallsThroughTo": None,
            },
            {
                "macro": "loadMapFadeIn",
                "handler": handler_by_macro["loadMapFadeIn"]["name"],
                "address": handler_by_macro["loadMapFadeIn"]["address"],
                "opcode": macros["loadMapFadeIn"]["opcode"],
                "cursorReadWidths": [],
                "mapEventTypeValue": None,
                "mapEventClearByteValue": None,
                "d1Immediate": None,
                "packedCoordinateMultiplier": None,
                "directServiceCalls": [],
                "fallsThroughTo": "csc48_loadMap",
            },
            {
                "macro": "reloadMap",
                "handler": handler_by_macro["reloadMap"]["name"],
                "address": handler_by_macro["reloadMap"]["address"],
                "opcode": macros["reloadMap"]["opcode"],
                "cursorReadWidths": [
                    field["widthBytes"] for field in macros["reloadMap"]["operandLayout"]
                ],
                "mapEventTypeValue": None,
                "mapEventClearByteValue": None,
                "d1Immediate": reload_selector_value,
                "packedCoordinateMultiplier": reload_scale_value,
                "directServiceCalls": [],
                "fallsThroughTo": None,
            },
            {
                "macro": "mapLoad",
                "handler": handler_by_macro["mapLoad"]["name"],
                "address": handler_by_macro["mapLoad"]["address"],
                "opcode": macros["mapLoad"]["opcode"],
                "cursorReadWidths": [
                    field["widthBytes"] for field in macros["mapLoad"]["operandLayout"]
                ],
                "mapEventTypeValue": None,
                "mapEventClearByteValue": None,
                "d1Immediate": None,
                "packedCoordinateMultiplier": load_scale_value,
                "directServiceCalls": [],
                "fallsThroughTo": None,
            },
        ]
    )
    caller_rows = []
    for row in handler_rows:
        statements = _stable_handler_statements(disasm, _handler_by_name(handlers, row["handler"]))
        counts = {
            target: len(_direct_call_sites(statements, target))
            for target in TRANSITION_SERVICE_TARGETS
        }
        if any(count not in {0, 1} for count in counts.values()):
            raise ValueError(f"transition caller site count drift: {row['handler']}")
        caller_rows.append(
            {
                "handler": row["handler"],
                "instructionTargetSiteCounts": counts,
                "effectiveTargetSiteCounts": dict(counts),
            }
        )
        row["directServiceCalls"] = [
            target for target in TRANSITION_SERVICE_TARGETS if counts[target]
        ]
    totals = {
        target: sum(row["instructionTargetSiteCounts"][target] for row in caller_rows)
        for target in TRANSITION_SERVICE_TARGETS
    }
    internal_handler_names = {handler["name"] for handler in handlers}
    resolutions = [
        {
            "instructionTarget": target,
            "effectiveTarget": target,
            "effectiveTargetScope": "internal" if target in internal_handler_names else "external",
        }
        for target in TRANSITION_SERVICE_TARGETS
    ]
    return {
        "macros": [
            {
                "name": name,
                "opcode": macros[name]["opcode"],
                "encodedBytes": macros[name]["encodedBytes"],
                "operandBytes": macros[name]["operandBytes"],
                "operandLayout": macros[name]["operandLayout"],
                "parameterOrdinals": macros[name]["parameterOrdinals"],
                "handler": TRANSITION_HANDLER_BY_MACRO[name],
                "sourceCommandCount": source_counts[name],
            }
            for name in TRANSITION_MACROS
        ],
        "sourceSites": sites,
        "programTotals": program_totals,
        "canonicalMapDomain": {
            "contractId": map_content["id"],
            "mapCount": map_content["summary"]["mapCount"],
            "mapIds": sorted(canonical_map_ids),
            "sourceMapCurrentValue": equates["MAP_CURRENT"],
        },
        "handlers": handler_rows,
        "callerBreakdown": {
            "callerHandlers": caller_rows,
            "targetResolutions": resolutions,
            "instructionTargetTotals": totals,
            "effectiveTargetTotals": dict(totals),
            "internalEffectiveTargetTotals": {
                target: totals[target] if target in internal_handler_names else 0
                for target in TRANSITION_SERVICE_TARGETS
            },
            "externalEffectiveTargetTotals": {
                target: totals[target] if target not in internal_handler_names else 0
                for target in TRANSITION_SERVICE_TARGETS
            },
        },
        "runtimeQuestions": TRANSITION_RUNTIME_QUESTIONS,
    }


def _force_state_direct_calls(statements: list[str]) -> list[dict[str, str]]:
    """Parse only call instructions; comments, labels, and operands do not count."""
    calls: list[dict[str, str]] = []
    pattern = re.compile(
        r"^(?P<opcode>bsr|jsr)(?:\.[bwls])?\s+\(?(?P<target>[A-Za-z_][A-Za-z0-9_]*)\)?(?:\.[bwls])?(?:\s|$)"
    )
    for raw_statement in statements:
        statement = raw_statement.split(";", 1)[0].strip()
        match = pattern.fullmatch(statement)
        if match is not None and not re.fullmatch(r"[ad][0-7]", match.group("target")):
            calls.append(
                {
                    "opcode": match.group("opcode"),
                    "instructionTarget": match.group("target"),
                }
            )
    return calls


def _force_state_aliases(
    disasm: Path, instruction_targets: set[str], addresses: dict[str, int], rom: bytes
) -> dict[str, dict[str, str]]:
    """Resolve the jump-interface spelling used by this bounded caller inventory."""
    aliases: dict[str, dict[str, str]] = {}
    interface_root = disasm / "code/common/tech/jumpinterfaces"
    for path in sorted(interface_root.rglob("*.asm"), key=lambda item: item.as_posix()):
        pending_label: str | None = None
        for raw in read_upstream_text(path).splitlines():
            statement = raw.split(";", 1)[0].strip()
            if not statement:
                continue
            label = re.fullmatch(r"([A-Za-z_][A-Za-z0-9_]*):", statement)
            if label is not None:
                pending_label = label.group(1)
                continue
            if pending_label not in instruction_targets:
                pending_label = None
                continue
            jump = re.fullmatch(
                r"jmp(?:\.[bwls])?\s+(?P<target>[A-Za-z_][A-Za-z0-9_]*)(?:\(pc\))?",
                statement,
            )
            if jump is None:
                raise ValueError(
                    "force-state jump-interface alias instruction drift: "
                    f"{pending_label}"
                )
            target = jump.group("target")
            if pending_label not in addresses or target not in addresses:
                raise ValueError(
                    "force-state jump-interface alias address is missing: "
                    f"{pending_label} -> {target}"
                )
            alias_address = addresses[pending_label]
            instruction = rom[alias_address : alias_address + 4]
            if len(instruction) != 4 or instruction[:2] != b"\x4e\xfa":
                raise ValueError(
                    f"force-state jump-interface ROM opcode drift: {pending_label}"
                )
            rom_target = alias_address + 2 + int.from_bytes(
                instruction[2:], "big", signed=True
            )
            if rom_target != addresses[target]:
                raise ValueError(
                    "force-state jump-interface source/ROM target drift: "
                    f"{pending_label} -> {target}"
                )
            aliases[pending_label] = {
                "effectiveTarget": target,
                "sourcePath": path.relative_to(disasm).as_posix(),
            }
            pending_label = None
    expected = {target for target in instruction_targets if target.startswith("j_")}
    if set(aliases) != expected:
        raise ValueError(
            "force-state jump-interface alias coverage drift: "
            f"expected {sorted(expected)}, got {sorted(aliases)}"
        )
    return dict(sorted(aliases.items()))


def _force_state_program_facts(
    program_corpus: dict[str, Any],
    *,
    macro_names: tuple[str, ...] = FORCE_STATE_MACRO_NAMES,
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    """Retain every program, including zero-use rows, for one bounded macro group."""
    source_sites: list[dict[str, Any]] = []
    program_totals: list[dict[str, Any]] = []
    for program in program_corpus["programs"]:
        command_rows = []
        counts: Counter[str] = Counter()
        for command in program["commands"]:
            macro = command["macro"]
            if macro not in macro_names:
                continue
            counts[macro] += 1
            command_rows.append(
                {
                    "commandIndex": command["index"],
                    "sourceLine": command["sourceLine"],
                    "macro": macro,
                    "arguments": command["arguments"],
                }
            )
        program_totals.append(
            {
                "programId": program["id"],
                "commandCount": sum(counts.values()),
                "macroCounts": {name: counts[name] for name in macro_names},
            }
        )
        if command_rows:
            source_sites.append({"programId": program["id"], "commands": command_rows})
    if len(program_totals) != program_corpus["summary"]["programCount"]:
        raise ValueError("bounded macro group zero-inclusive program domain drift")
    return source_sites, program_totals


def _force_state_ordered_statements(
    statements: list[str], patterns: list[str], *, owner: str
) -> list[str]:
    """Guard one named section's relevant instruction order, not a file fragment."""
    position = 0
    matched: list[str] = []
    for pattern in patterns:
        index, _ = _next_statement(statements, position, pattern, owner=owner)
        matched.append(statements[index])
        position = index + 1
    return matched


def _force_state_section_guard(
    macro: str, statements: list[str], equates: dict[str, int]
) -> dict[str, Any]:
    """Parse branch polarity and mutation/call ordering in one bounded handler section."""
    if macro == "join":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w #0,\(\(CURRENT_SPEECH_SFX-\$1000000\)\)\.w",
                r"jsr \(WaitForViewScrollEnd\)\.w",
                r"move\.w \(a6\)\+,d0",
                r"bclr #(?P<join_bit>\d+),d0",
                r"bne\.s [A-Za-z_][A-Za-z0-9_]*",
                r"sndCom MUSIC_JOIN",
                r"bra\.s [A-Za-z_][A-Za-z0-9_]*",
                r"sndCom MUSIC_SAD_JOIN",
                r"cmpi\.w #(?P<special_selector>\d+),d0",
                r"bne\.s [A-Za-z_][A-Za-z0-9_]*",
                r"move\.w #ALLY_SARAH,d0",
                r"jsr j_JoinForce",
                r"move\.w #ALLY_CHESTER,d0",
                r"jsr j_JoinForce",
                r"txt 447",
                r"bra\.s [A-Za-z_][A-Za-z0-9_]*",
                r"jsr j_JoinForce",
                r"jsr j_GetClass",
                r"move\.w d0,\(\(DIALOGUE_NAME_INDEX_1-\$1000000\)\)\.w",
                r"move\.w d1,\(\(DIALOGUE_NAME_INDEX_2-\$1000000\)\)\.w",
                r"txt 446",
                r"jsr j_FadeOut_WaitForP1Input",
                r"clsTxt",
                r"moveq #10,d0",
                r"jsr \(Sleep\)\.w",
                r"rts",
            ],
            owner="csc08_joinForce",
        )
        selector = _literal(re.fullmatch(r"cmpi\.w #(?P<value>\d+),d0", ordered[8]).group("value"))
        if selector != equates["COMBATANT_ENEMIES_START"]:
            raise ValueError("force-state join special-selector source use drift")
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[3],
                    "branchInstruction": ordered[4],
                    "fallthroughInstruction": ordered[5],
                    "branchTargetInstruction": ordered[7],
                },
                {
                    "testInstruction": ordered[8],
                    "branchInstruction": ordered[9],
                    "fallthroughInstruction": ordered[10],
                    "branchTargetInstruction": ordered[16],
                },
            ],
            "sourceConstantUses": [
                {
                    "constant": "COMBATANT_ENEMIES_START",
                    "value": selector,
                    "instruction": ordered[8],
                },
                {
                    "constant": "ALLY_SARAH",
                    "value": equates["ALLY_SARAH"],
                    "instruction": ordered[10],
                },
                {
                    "constant": "ALLY_CHESTER",
                    "value": equates["ALLY_CHESTER"],
                    "instruction": ordered[12],
                },
            ],
        }
    if macro == "jumpIfDefeatedByLastAttack":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w \(a6\)\+,d0",
                r"lea \(\(DEAD_COMBATANTS_LIST-\$1000000\)\)\.w,a1",
                r"move\.w \(\(DEAD_COMBATANTS_LIST_LENGTH-\$1000000\)\)\.w,d7",
                r"subq\.w #1,d7",
                r"bcs\.w [A-Za-z_][A-Za-z0-9_]*",
                r"cmp\.b \(a1\)\+,d0",
                r"beq\.w [A-Za-z_][A-Za-z0-9_]*",
                r"dbf d7,[A-Za-z_][A-Za-z0-9_]*",
                r"addq\.w #4,a6",
                r"bra\.w [A-Za-z_][A-Za-z0-9_]*",
                r"movea\.l \(a6\),a6",
                r"rts",
            ],
            owner="csc0E_jumpIfForceMemberInList",
        )
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[3],
                    "branchInstruction": ordered[4],
                    "fallthroughInstruction": ordered[5],
                    "branchTargetInstruction": ordered[11],
                },
                {
                    "testInstruction": ordered[5],
                    "branchInstruction": ordered[6],
                    "fallthroughInstruction": ordered[7],
                    "branchTargetInstruction": ordered[10],
                },
            ],
            "sourceConstantUses": [],
        }
    if macro == "jumpIfDead":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w \(a6\)\+,d0",
                r"jsr j_GetCurrentHp",
                r"tst\.w d1",
                r"bne\.w [A-Za-z_][A-Za-z0-9_]*",
                r"movea\.l \(a6\),a6",
                r"bra\.s [A-Za-z_][A-Za-z0-9_]*",
                r"addq\.w #4,a6",
                r"rts",
            ],
            owner="csc0F_jumpIfCharacterDead",
        )
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[2],
                    "branchInstruction": ordered[3],
                    "fallthroughInstruction": ordered[4],
                    "branchTargetInstruction": ordered[6],
                }
            ],
            "sourceConstantUses": [],
        }
    if macro == "allyDefeated":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"lea \(\(DEAD_COMBATANTS_LIST-\$1000000\)\)\.w,a1",
                r"adda\.w \(\(DEAD_COMBATANTS_LIST_LENGTH-\$1000000\)\)\.w,a1",
                r"move\.w \(a6\)\+,d0",
                r"move\.b d0,\(a1\)",
                r"addq\.w #1,\(\(DEAD_COMBATANTS_LIST_LENGTH-\$1000000\)\)\.w",
                r"rts",
            ],
            owner="csc1F_addDefeatedAlly",
        )
        return {"orderedInstructions": ordered, "branchRecords": [], "sourceConstantUses": []}
    if macro == "updateDefeatedAllies":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"lea \(\(DEAD_COMBATANTS_LIST-\$1000000\)\)\.w,a1",
                r"move\.w \(\(DEAD_COMBATANTS_LIST_LENGTH-\$1000000\)\)\.w,d2",
                r"adda\.w d2,a1",
                r"moveq #(?P<start>\$?[0-9A-Fa-f]+),d0",
                r"moveq #(?P<loop>\$?[0-9A-Fa-f]+),d7",
                r"jsr j_GetCombatantX",
                r"cmpi\.w #(?P<not_found>-?\d+),d1",
                r"beq\.s [A-Za-z_][A-Za-z0-9_]*",
                r"move\.b d0,\(a1\)\+",
                r"addq\.w #1,d2",
                r"addq\.b #1,d0",
                r"dbf d7,[A-Za-z_][A-Za-z0-9_]*",
                r"move\.w d2,\(\(DEAD_COMBATANTS_LIST_LENGTH-\$1000000\)\)\.w",
                r"rts",
            ],
            owner="csc20_updateDefeatedAllies",
        )
        start_match = re.fullmatch(r"moveq #(?P<value>\$?[0-9A-Fa-f]+),d0", ordered[3])
        if start_match is None:
            raise ValueError("force-state defeated scan start use shape drift")
        start = _literal(start_match.group("value"))
        if start & 0xFF != equates["COMBATANT_ENEMIES_START"]:
            raise ValueError("force-state defeated scan start source use drift")
        comparison_match = re.fullmatch(r"cmpi\.w #(?P<value>-?\d+),d1", ordered[6])
        if comparison_match is None:
            raise ValueError("force-state defeated comparison use shape drift")
        comparison = _literal(comparison_match.group("value"))
        if comparison != -1:
            raise ValueError("force-state defeated comparison operand drift")
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[6],
                    "branchInstruction": ordered[7],
                    "fallthroughInstruction": ordered[8],
                    "branchTargetInstruction": ordered[10],
                }
            ],
            "sourceConstantUses": [
                {
                    "constant": "COMBATANT_ENEMIES_START",
                    "value": equates["COMBATANT_ENEMIES_START"],
                    "instruction": ordered[3],
                }
            ],
        }
    if macro == "reviveAlly":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w \(a6\)\+,d0",
                r"lea \(\(DEAD_COMBATANTS_LIST-\$1000000\)\)\.w,a1",
                r"lea \(\(DEAD_COMBATANTS_LIST-\$1000000\)\)\.w,a2",
                r"move\.w \(\(DEAD_COMBATANTS_LIST_LENGTH-\$1000000\)\)\.w,d7",
                r"subq\.w #1,d7",
                r"bcs\.w [A-Za-z_][A-Za-z0-9_]*",
                r"cmp\.b \(a1\),d0",
                r"bne\.s [A-Za-z_][A-Za-z0-9_]*",
                r"addq\.l #1,a1",
                r"subq\.w #1,\(\(DEAD_COMBATANTS_LIST_LENGTH-\$1000000\)\)\.w",
                r"bra\.s [A-Za-z_][A-Za-z0-9_]*",
                r"move\.b \(a1\)\+,\(a2\)\+",
                r"dbf d7,[A-Za-z_][A-Za-z0-9_]*",
                r"rts",
            ],
            owner="csc21_reviveAlly",
        )
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[4],
                    "branchInstruction": ordered[5],
                    "fallthroughInstruction": ordered[6],
                    "branchTargetInstruction": ordered[13],
                },
                {
                    "testInstruction": ordered[6],
                    "branchInstruction": ordered[7],
                    "fallthroughInstruction": ordered[8],
                    "branchTargetInstruction": ordered[11],
                },
            ],
            "sourceConstantUses": [],
        }
    raise ValueError(f"force-state handler guard has no macro profile: {macro}")


def _active_party_section_guard(
    macro: str, statements: list[str], equates: dict[str, int]
) -> dict[str, Any]:
    """Guard the four adjacent active-party handler sections at their use sites."""
    if macro == "joinBatParty":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w #(?P<value>-?\d+),\(\(DIALOGUE_NAME_INDEX_1-\$1000000\)\)\.w",
                r"move\.w \(a6\)\+,d0",
                r"jsr j_IsInBattleParty",
                r"bne\.w @[A-Za-z_][A-Za-z0-9_]*",
                r"move\.w d0,d6",
                r"jsr j_UpdateForce",
                r"lea \(\(BATTLE_PARTY_MEMBERS-\$1000000\)\)\.w,a0",
                r"move\.w \(\(BATTLE_PARTY_MEMBERS_NUMBER-\$1000000\)\)\.w,d7",
                r"subq\.w #(?P<value>\d+),d7",
                r"move\.b \(a0\),d0",
                r"jsr j_GetCurrentHp",
                r"tst\.w d1",
                r"beq\.w @[A-Za-z_][A-Za-z0-9_]*",
                r"addq\.l #1,a0",
                r"dbf d7,@[A-Za-z_][A-Za-z0-9_]*",
                r"move\.b \(a0\),d0",
                r"move\.w d0,\(\(DIALOGUE_NAME_INDEX_1-\$1000000\)\)\.w",
                r"jsr j_LeaveBattleParty",
                r"move\.b d6,d0",
                r"jsr j_JoinBattleParty",
                r"rts",
            ],
            owner="csc51_joinBattleParty",
        )
        initialization_match = re.fullmatch(
            r"move\.w #(?P<value>-?\d+),\(\(DIALOGUE_NAME_INDEX_1-\$1000000\)\)\.w",
            ordered[0],
        )
        decrement_match = re.fullmatch(r"subq\.w #(?P<value>\d+),d7", ordered[8])
        if initialization_match is None or decrement_match is None:
            raise ValueError("active-party battle-party loop decrement use shape drift")
        if _literal(initialization_match.group("value")) != -1:
            raise ValueError("active-party battle-party initialization source use drift")
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[2],
                    "branchInstruction": ordered[3],
                    "fallthroughInstruction": ordered[4],
                    "branchTargetInstruction": ordered[20],
                },
                {
                    "testInstruction": ordered[11],
                    "branchInstruction": ordered[12],
                    "fallthroughInstruction": ordered[13],
                    "branchTargetInstruction": ordered[15],
                },
            ],
            "sourceConstantUses": [],
            "sourceExpressionUses": [],
            "sourceLiteralUses": [
                {"value": _literal(initialization_match.group("value")), "instruction": ordered[0]},
                {"value": _literal(decrement_match.group("value")), "instruction": ordered[8]},
            ],
            "mutationCallOrder": [ordered[0], ordered[16], ordered[17], ordered[19]],
        }
    if macro == "joinForceAI":
        if "AIBITFIELD_AI_CONTROLLED" not in equates:
            raise ValueError("active-party AI-control source constant is missing")
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w \(a6\)\+,d0",
                r"jsr j_GetActivationBitfield",
                r"move\.w \(a6\)\+,d2",
                r"bne\.s @[A-Za-z_][A-Za-z0-9_]*",
                r"andi\.w #\(\$FFFF-AIBITFIELD_AI_CONTROLLED\),d1",
                r"bra\.s @[A-Za-z_][A-Za-z0-9_]*",
                r"ori\.w #AIBITFIELD_AI_CONTROLLED,d1",
                r"jsr j_JoinForce",
                r"jsr j_SetActivationBitfield",
                r"rts",
            ],
            owner="csc54_joinForceAi",
        )
        clear_mask = 0xFFFF - equates["AIBITFIELD_AI_CONTROLLED"]
        if clear_mask < 0:
            raise ValueError("active-party AI-control clear mask source relation drift")
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[2],
                    "branchInstruction": ordered[3],
                    "fallthroughInstruction": ordered[4],
                    "branchTargetInstruction": ordered[6],
                }
            ],
            "sourceConstantUses": [
                {
                    "constant": "AIBITFIELD_AI_CONTROLLED",
                    "value": equates["AIBITFIELD_AI_CONTROLLED"],
                    "instruction": ordered[4],
                },
                {
                    "constant": "AIBITFIELD_AI_CONTROLLED",
                    "value": equates["AIBITFIELD_AI_CONTROLLED"],
                    "instruction": ordered[6],
                },
            ],
            "sourceExpressionUses": [
                {
                    "expression": "($FFFF-AIBITFIELD_AI_CONTROLLED)",
                    "resolvedValue": clear_mask,
                    "instruction": ordered[4],
                }
            ],
            "sourceLiteralUses": [],
            "mutationCallOrder": [ordered[4], ordered[6], ordered[7], ordered[8]],
        }
    if macro == "resetForceBattleStats":
        ordered = _force_state_ordered_statements(
            statements,
            [r"jsr ResetAlliesBattleStats", r"rts"],
            owner="csc55_resetCharacterBattleStats",
        )
        return {
            "orderedInstructions": ordered,
            "branchRecords": [],
            "sourceConstantUses": [],
            "sourceExpressionUses": [],
            "sourceLiteralUses": [],
            "mutationCallOrder": [ordered[0]],
        }
    if macro == "addNewFollower":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w \(a6\)\+,d0",
                r"bsr\.w GetEntityAddressFromCharacter",
                r"moveq #0,d1",
                r"lea \(\(EXPLORATION_ENTITIES-\$1000000\)\)\.w,a0",
                r"cmpi\.b #(?P<sentinel>-?\d+),\(a0\)",
                r"beq\.w @[A-Za-z_][A-Za-z0-9_]*",
                r"move\.b \(a0\)\+,d1",
                r"bra\.s @[A-Za-z_][A-Za-z0-9_]*",
                r"move\.w #(?P<d2>\$?[0-9A-Fa-f]+),d2",
                r"move\.w #(?P<d3>\$?[0-9A-Fa-f]+),d3",
                r"jsr AddFollower",
                r"rts",
            ],
            owner="csc56_addFollower",
        )
        sentinel_match = re.fullmatch(r"cmpi\.b #(?P<value>-?\d+),\(a0\)", ordered[4])
        d2_match = re.fullmatch(r"move\.w #(?P<value>\$?[0-9A-Fa-f]+),d2", ordered[8])
        d3_match = re.fullmatch(r"move\.w #(?P<value>\$?[0-9A-Fa-f]+),d3", ordered[9])
        if sentinel_match is None or d2_match is None or d3_match is None:
            raise ValueError("active-party follower literal use shape drift")
        sentinel = _literal(sentinel_match.group("value"))
        if sentinel != -1:
            raise ValueError("active-party follower sentinel source use drift")
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[4],
                    "branchInstruction": ordered[5],
                    "fallthroughInstruction": ordered[6],
                    "branchTargetInstruction": ordered[8],
                }
            ],
            "sourceConstantUses": [],
            "sourceExpressionUses": [],
            "sourceLiteralUses": [
                {"value": sentinel, "instruction": ordered[4]},
                {"value": _literal(d2_match.group("value")), "instruction": ordered[8]},
                {"value": _literal(d3_match.group("value")), "instruction": ordered[9]},
            ],
            "mutationCallOrder": [ordered[6], ordered[8], ordered[9], ordered[10]],
        }
    raise ValueError(f"active-party handler guard has no macro profile: {macro}")


def _force_state_join_comment_bit(
    disasm: Path, handler: dict[str, Any], section_guard: dict[str, Any]
) -> None:
    """Cross-check the nearby source comment against the guarded join bit use site."""
    source = read_upstream_text(disasm / handler["sourcePath"])
    label = re.search(r"^csc08_joinForce:\s*$", source, re.MULTILINE)
    if label is None:
        raise ValueError("force-state join section label is missing")
    comments = list(
        re.finditer(
            r"^\s*;.*?\bbit (?P<bit>\d+) set for sad join music\s*$",
            source[: label.start()],
            re.MULTILINE,
        )
    )
    if not comments:
        raise ValueError("force-state join bit source comment is missing")
    bit = int(comments[-1].group("bit"))
    instruction = section_guard["branchRecords"][0]["testInstruction"]
    match = re.fullmatch(r"bclr #(?P<bit>\d+),d0", instruction)
    if match is None or int(match.group("bit")) != bit:
        raise ValueError("force-state join bit comment/use-site drift")


def _force_state_caller_breakdown(
    disasm: Path,
    handlers: list[dict[str, Any]],
    handler_names: tuple[str, ...],
    direct_call_rows: dict[str, list[dict[str, str]]],
    addresses: dict[str, int],
    rom: bytes,
) -> dict[str, Any]:
    """Keep direct/effective call identities and zero rows for a named handler group."""
    instruction_targets = sorted(
        {call["instructionTarget"] for calls in direct_call_rows.values() for call in calls}
    )
    aliases = _force_state_aliases(disasm, set(instruction_targets), addresses, rom)
    bounded_handlers = {
        handler["name"]: handler
        for handler in (_handler_by_name(handlers, name) for name in handler_names)
    }
    target_resolutions = []
    for target in instruction_targets:
        effective_target = aliases.get(target, {}).get("effectiveTarget", target)
        effective_target_owner = bounded_handlers.get(effective_target)
        target_resolutions.append(
            {
                "instructionTarget": target,
                "effectiveTarget": effective_target,
                "aliasSourcePath": aliases.get(target, {}).get("sourcePath"),
                "effectiveTargetScope": (
                    "internal" if effective_target_owner is not None else "external"
                ),
            }
        )
    effective_targets = sorted({row["effectiveTarget"] for row in target_resolutions})
    if len(effective_targets) != len(target_resolutions):
        raise ValueError("force-state effective target declaration is ambiguous")
    resolved_by_instruction = {
        row["instructionTarget"]: row["effectiveTarget"] for row in target_resolutions
    }
    caller_handlers = []
    for handler_name in handler_names:
        calls = direct_call_rows[handler_name]
        instruction_counts = {target: 0 for target in instruction_targets}
        effective_counts = {target: 0 for target in effective_targets}
        for call in calls:
            instruction_counts[call["instructionTarget"]] += 1
            effective_counts[resolved_by_instruction[call["instructionTarget"]]] += 1
        caller_handlers.append(
            {
                "handler": handler_name,
                "instructionTargetSiteCounts": instruction_counts,
                "effectiveTargetSiteCounts": effective_counts,
            }
        )
    instruction_totals = {
        target: sum(row["instructionTargetSiteCounts"][target] for row in caller_handlers)
        for target in instruction_targets
    }
    effective_totals = {
        target: sum(row["effectiveTargetSiteCounts"][target] for row in caller_handlers)
        for target in effective_targets
    }
    scope_by_effective_target = {
        row["effectiveTarget"]: row["effectiveTargetScope"] for row in target_resolutions
    }
    if set(scope_by_effective_target) != set(effective_totals):
        raise ValueError("force-state effective target scope coverage drift")

    def scoped_effective_totals(scope: str) -> dict[str, int]:
        return {
            target: effective_totals[target]
            if scope_by_effective_target[target] == scope
            else 0
            for target in effective_totals
        }

    return {
        "callerHandlers": caller_handlers,
        "targetResolutions": target_resolutions,
        "instructionTargetTotals": instruction_totals,
        "effectiveTargetTotals": effective_totals,
        "internalEffectiveTargetTotals": scoped_effective_totals("internal"),
        "externalEffectiveTargetTotals": scoped_effective_totals("external"),
    }


def _force_state_command_facts(
    disasm: Path,
    equates: dict[str, int],
    macros: dict[str, dict[str, Any]],
    dispatch_targets: list[str],
    handlers: list[dict[str, Any]],
    program_corpus: dict[str, Any],
    addresses: dict[str, int],
    rom: bytes,
    upstream_path: Path,
) -> dict[str, Any]:
    """Build the six-command roster/death source-site and handler identity spine."""
    required_constants = (
        "ALLY_SARAH",
        "ALLY_CHESTER",
        "COMBATANT_ENEMIES_START",
    )
    missing = [name for name in required_constants if name not in equates]
    if missing:
        raise ValueError(f"force-state source constants are missing: {missing}")
    macro_to_handler = dict(zip(FORCE_STATE_MACRO_NAMES, FORCE_STATE_HANDLER_NAMES, strict=True))
    source_sites, program_totals = _force_state_program_facts(program_corpus)
    source_counts: Counter[str] = Counter()
    for site in source_sites:
        source_counts.update(command["macro"] for command in site["commands"])
    for macro in FORCE_STATE_MACRO_NAMES:
        if sum(row["macroCounts"][macro] for row in program_totals) != source_counts[macro]:
            raise ValueError(f"force-state program total drift: {macro}")

    handler_rows = []
    direct_call_rows: dict[str, list[dict[str, str]]] = {}
    for macro, handler_name in macro_to_handler.items():
        contract = macros[macro]
        if contract["kind"] != "command" or contract["aliasOf"] is not None:
            raise ValueError(f"force-state macro is not primary: {macro}")
        opcode = contract["opcode"]
        if opcode is None or dispatch_targets[opcode] != handler_name:
            raise ValueError(f"force-state dispatcher target drift: {macro}")
        handler = _handler_by_name(handlers, handler_name)
        if (
            handler["opcodes"] != [opcode]
            or handler["encodedCommandBytes"] != contract["encodedBytes"]
        ):
            raise ValueError(f"force-state handler ABI drift: {handler_name}")
        statements = _stable_handler_statements(disasm, handler)
        direct_calls = _force_state_direct_calls(statements)
        section_guard = _force_state_section_guard(macro, statements, equates)
        if macro == "join":
            _force_state_join_comment_bit(disasm, handler, section_guard)
        direct_call_rows[handler_name] = direct_calls
        handler_rows.append(
            {
                "macro": macro,
                "handler": handler_name,
                "address": handler["address"],
                "opcode": opcode,
                "cursorReadWidths": [
                    field["widthBytes"] for field in contract["operandLayout"]
                ],
                "statementCount": len(statements),
                "guardedStatements": statements,
                "sectionGuard": section_guard,
                "directCalls": direct_calls,
            }
        )
    caller_breakdown = _force_state_caller_breakdown(
        disasm,
        handlers,
        FORCE_STATE_HANDLER_NAMES,
        direct_call_rows,
        addresses,
        rom,
    )

    stats = build_stats_inventory(upstream_path)
    battleparty = next(
        row for row in stats["files"] if row["path"] == "code/common/stats/battleparty.asm"
    )
    required_services = {"JoinForce", "UpdateForce"}
    if not required_services <= set(battleparty["globalLabels"]):
        raise ValueError("force-state common-stats battleparty identity drift")
    return {
        "macros": [
            {
                "name": name,
                "opcode": macros[name]["opcode"],
                "encodedBytes": macros[name]["encodedBytes"],
                "operandBytes": macros[name]["operandBytes"],
                "operandLayout": macros[name]["operandLayout"],
                "parameterOrdinals": macros[name]["parameterOrdinals"],
                "handler": macro_to_handler[name],
                "sourceCommandCount": source_counts[name],
            }
            for name in FORCE_STATE_MACRO_NAMES
        ],
        "sourceSites": source_sites,
        "programTotals": program_totals,
        "handlers": handler_rows,
        "callerBreakdown": caller_breakdown,
        "commonStatsIdentity": {
            "contractId": stats["id"],
            "upstreamCommit": stats["upstream"]["commit"],
            "sourcePath": battleparty["path"],
            "sourceSha256": battleparty["sha256"],
            "services": sorted(required_services),
        },
        "runtimeQuestions": FORCE_STATE_RUNTIME_QUESTIONS,
    }


def _active_party_source_identity_joins(
    disasm: Path, upstream_path: Path
) -> dict[str, Any]:
    """Join only the source owners that the active-party handlers call directly."""
    stats = build_stats_inventory(upstream_path)
    stats_sources = {
        row["path"]: row
        for row in stats["files"]
        if row["path"]
        in {
            "code/common/stats/battleparty.asm",
            "code/common/stats/combatantstats_1.asm",
            "code/common/stats/combatantstats_2.asm",
        }
    }
    expected_stats_symbols = {
        "code/common/stats/battleparty.asm": [
            "IsInBattleParty",
            "JoinBattleParty",
            "JoinForce",
            "LeaveBattleParty",
            "UpdateForce",
        ],
        "code/common/stats/combatantstats_1.asm": ["GetActivationBitfield"],
        "code/common/stats/combatantstats_2.asm": ["SetActivationBitfield"],
    }
    if set(stats_sources) != set(expected_stats_symbols):
        raise ValueError("active-party common-stats source identity coverage drift")
    common_stats_sources = []
    for path, symbols in expected_stats_symbols.items():
        row = stats_sources[path]
        if not set(symbols) <= set(row["globalLabels"]):
            raise ValueError(f"active-party common-stats symbol identity drift: {path}")
        common_stats_sources.append(
            {"sourcePath": path, "sourceSha256": row["sha256"], "symbols": symbols}
        )

    follower_path = "code/common/scripting/entity/entityfunctions_2.asm"
    follower_source = read_upstream_text(disasm / follower_path)
    if re.search(r"^AddFollower:\s*$", follower_source, re.MULTILINE) is None:
        raise ValueError("active-party follower owner symbol identity drift")
    battle_stats_path = "code/common/scripting/map/resetalliesstats.asm"
    battle_stats_source = read_upstream_text(disasm / battle_stats_path)
    if re.search(r"^ResetAlliesBattleStats:\s*$", battle_stats_source, re.MULTILINE) is None:
        raise ValueError("active-party battle-stats owner symbol identity drift")
    return {
        "commonStats": {
            "contractId": stats["id"],
            "upstreamCommit": stats["upstream"]["commit"],
            "sources": common_stats_sources,
        },
        "followerOwner": {
            "sourcePath": follower_path,
            "sourceSha256": hashlib.sha256(follower_source.encode()).hexdigest().upper(),
            "symbols": ["AddFollower"],
        },
        "battleStatsOwner": {
            "sourcePath": battle_stats_path,
            "sourceSha256": hashlib.sha256(battle_stats_source.encode()).hexdigest().upper(),
            "symbols": ["ResetAlliesBattleStats"],
        },
    }


def _active_party_command_facts(
    disasm: Path,
    equates: dict[str, int],
    macros: dict[str, dict[str, Any]],
    dispatch_targets: list[str],
    handlers: list[dict[str, Any]],
    program_corpus: dict[str, Any],
    addresses: dict[str, int],
    rom: bytes,
    upstream_path: Path,
) -> dict[str, Any]:
    """Build the active-party/AI/follower four-command static contract."""
    macro_to_handler = dict(zip(ACTIVE_PARTY_MACRO_NAMES, ACTIVE_PARTY_HANDLER_NAMES, strict=True))
    source_sites, program_totals = _force_state_program_facts(
        program_corpus, macro_names=ACTIVE_PARTY_MACRO_NAMES
    )
    source_counts: Counter[str] = Counter()
    for site in source_sites:
        source_counts.update(command["macro"] for command in site["commands"])
    for macro in ACTIVE_PARTY_MACRO_NAMES:
        if sum(row["macroCounts"][macro] for row in program_totals) != source_counts[macro]:
            raise ValueError(f"active-party program total drift: {macro}")

    handler_rows = []
    direct_call_rows: dict[str, list[dict[str, str]]] = {}
    for macro, handler_name in macro_to_handler.items():
        contract = macros[macro]
        if contract["kind"] != "command" or contract["aliasOf"] is not None:
            raise ValueError(f"active-party macro is not primary: {macro}")
        opcode = contract["opcode"]
        if opcode is None or dispatch_targets[opcode] != handler_name:
            raise ValueError(f"active-party dispatcher target drift: {macro}")
        handler = _handler_by_name(handlers, handler_name)
        if (
            handler["opcodes"] != [opcode]
            or handler["encodedCommandBytes"] != contract["encodedBytes"]
        ):
            raise ValueError(f"active-party handler ABI drift: {handler_name}")
        statements = _stable_handler_statements(disasm, handler)
        direct_calls = _force_state_direct_calls(statements)
        direct_call_rows[handler_name] = direct_calls
        handler_rows.append(
            {
                "macro": macro,
                "handler": handler_name,
                "address": handler["address"],
                "opcode": opcode,
                "cursorReadWidths": [
                    field["widthBytes"] for field in contract["operandLayout"]
                ],
                "statementCount": len(statements),
                "guardedStatements": statements,
                "sectionGuard": _active_party_section_guard(macro, statements, equates),
                "directCalls": direct_calls,
            }
        )
    return {
        "macros": [
            {
                "name": name,
                "opcode": macros[name]["opcode"],
                "encodedBytes": macros[name]["encodedBytes"],
                "operandBytes": macros[name]["operandBytes"],
                "operandLayout": macros[name]["operandLayout"],
                "parameterOrdinals": macros[name]["parameterOrdinals"],
                "handler": macro_to_handler[name],
                "sourceCommandCount": source_counts[name],
            }
            for name in ACTIVE_PARTY_MACRO_NAMES
        ],
        "sourceSites": source_sites,
        "programTotals": program_totals,
        "handlers": handler_rows,
        "callerBreakdown": _force_state_caller_breakdown(
            disasm,
            handlers,
            ACTIVE_PARTY_HANDLER_NAMES,
            direct_call_rows,
            addresses,
            rom,
        ),
        "sourceIdentityJoins": _active_party_source_identity_joins(disasm, upstream_path),
        "runtimeQuestions": ACTIVE_PARTY_RUNTIME_QUESTIONS,
    }


def _story_state_program_facts(
    program_corpus: dict[str, Any],
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]]]:
    """Reference the authoritative story-state rows from the bounded source sites."""
    source_sites, program_totals = _force_state_program_facts(
        program_corpus, macro_names=STORY_STATE_MACRO_NAMES
    )
    story_state = program_corpus["storyState"]
    field_by_macro = {
        "jumpIfFlagSet": "conditionalReads",
        "jumpIfFlagClear": "conditionalReads",
        "setF": "directWrites",
        "clearF": "directWrites",
        "yesNo": "yesNoPromptWrites",
        "setStoryFlag": "battleUnlockWrites",
    }
    references: dict[tuple[str, int], tuple[str, int, dict[str, Any]]] = {}
    for field in (
        "conditionalReads",
        "directWrites",
        "yesNoPromptWrites",
        "battleUnlockWrites",
    ):
        for entry_index, row in enumerate(story_state[field]):
            key = (row["program"], row["commandIndex"])
            if key in references:
                raise ValueError(f"story-state source reference is ambiguous: {key}")
            references[key] = (field, entry_index, row)

    source_commands = {
        (program["id"], command["index"]): command
        for program in program_corpus["programs"]
        for command in program["commands"]
    }

    source_keys: set[tuple[str, int]] = set()
    annotated_sites: list[dict[str, Any]] = []
    for site in source_sites:
        commands: list[dict[str, Any]] = []
        for command in site["commands"]:
            macro = command["macro"]
            if macro == "csc10":
                raise ValueError("story-state primary csc10 carrier unexpectedly has a source site")
            expected_field = field_by_macro.get(macro)
            if expected_field is None:
                raise ValueError(
                    "story-state source macro lacks an authoritative reference: "
                    f"{macro}"
                )
            key = (site["programId"], command["commandIndex"])
            source_keys.add(key)
            reference = references.get(key)
            if reference is None:
                raise ValueError(f"story-state source reference is missing: {key}")
            field, entry_index, row = reference
            if field != expected_field:
                raise ValueError(f"story-state source reference field drift: {key}")
            if field == "conditionalReads":
                expected_condition = "set" if macro == "jumpIfFlagSet" else "clear"
                if (
                    row["condition"] != expected_condition
                    or row["flag"] != _literal(command["arguments"][0])
                    or row["targetSymbol"] != source_commands[key]["targetSymbol"]
                ):
                    raise ValueError(f"story-state conditional read use-site drift: {key}")
            elif field == "directWrites":
                if row["macro"] != macro or row["flag"] != _literal(command["arguments"][0]):
                    raise ValueError(f"story-state direct write use-site drift: {key}")
            elif field == "battleUnlockWrites":
                if row["battle"] != _literal(command["arguments"][0]):
                    raise ValueError(f"story-state battle-unlock use-site drift: {key}")
            commands.append(
                {
                    **command,
                    "storyStateReference": {"field": field, "entryIndex": entry_index},
                }
            )
        annotated_sites.append({"programId": site["programId"], "commands": commands})
    if source_keys != set(references):
        raise ValueError("story-state bounded source/reference coverage drift")
    cross_checks = [
        {"field": field, "entryCount": len(story_state[field])}
        for field in (
            "conditionalReads",
            "directWrites",
            "yesNoPromptWrites",
            "battleUnlockWrites",
        )
    ]
    return annotated_sites, program_totals, cross_checks


def _story_state_section_guard(
    handler_name: str, statements: list[str], equates: dict[str, int]
) -> dict[str, Any]:
    """Guard branching and mutation order in one story-state handler section."""
    if handler_name == "csc0C_jumpIfFlagSet":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w \(a6\)\+,d1",
                r"jsr j_CheckFlag",
                r"beq\.w [A-Za-z_][A-Za-z0-9_]*",
                r"movea\.l \(a6\),a6",
                r"bra\.s [A-Za-z_][A-Za-z0-9_]*",
                r"addq\.w #(?P<value>\d+),a6",
                r"rts",
            ],
            owner=handler_name,
        )
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[1],
                    "branchInstruction": ordered[2],
                    "fallthroughInstruction": ordered[3],
                    "branchTargetInstruction": ordered[5],
                }
            ],
            "sourceConstantUses": [],
            "sourceLiteralUses": [
                {
                    "value": _literal(
                        re.fullmatch(r"addq\.w #(?P<value>\d+),a6", ordered[5]).group("value")
                    ),
                    "instruction": ordered[5],
                }
            ],
            "mutationCallOrder": [ordered[3], ordered[5]],
        }
    if handler_name == "csc0D_jumpIfFlagClear":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w \(a6\)\+,d1",
                r"jsr j_CheckFlag",
                r"bne\.w [A-Za-z_][A-Za-z0-9_]*",
                r"movea\.l \(a6\),a6",
                r"bra\.s [A-Za-z_][A-Za-z0-9_]*",
                r"addq\.w #(?P<value>\d+),a6",
                r"rts",
            ],
            owner=handler_name,
        )
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[1],
                    "branchInstruction": ordered[2],
                    "fallthroughInstruction": ordered[3],
                    "branchTargetInstruction": ordered[5],
                }
            ],
            "sourceConstantUses": [],
            "sourceLiteralUses": [
                {
                    "value": _literal(
                        re.fullmatch(r"addq\.w #(?P<value>\d+),a6", ordered[5]).group("value")
                    ),
                    "instruction": ordered[5],
                }
            ],
            "mutationCallOrder": [ordered[3], ordered[5]],
        }
    if handler_name == "csc10_toggleFlag":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w \(a6\)\+,d1",
                r"move\.w \(a6\)\+,d0",
                r"bne\.s [A-Za-z_][A-Za-z0-9_]*",
                r"jsr j_ClearFlag",
                r"bra\.s [A-Za-z_][A-Za-z0-9_]*",
                r"jsr j_SetFlag",
                r"rts",
            ],
            owner=handler_name,
        )
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[1],
                    "branchInstruction": ordered[2],
                    "fallthroughInstruction": ordered[3],
                    "branchTargetInstruction": ordered[5],
                }
            ],
            "sourceConstantUses": [],
            "sourceLiteralUses": [],
            "mutationCallOrder": [ordered[3], ordered[5]],
        }
    if handler_name == "csc11_promptYesNoForStoryFlow":
        constant = "FLAG_INDEX_YES_NO_PROMPT"
        if constant not in equates:
            raise ValueError("story-state yes/no source constant is missing")
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.l a6,-\(sp\)",
                r"jsr j_YesNoPrompt",
                r"movea\.l \(sp\)\+,a6",
                rf"moveq #{constant},d1",
                r"tst\.w d0",
                r"bne\.s [A-Za-z_][A-Za-z0-9_]*",
                r"jsr j_SetFlag",
                r"bra\.s [A-Za-z_][A-Za-z0-9_]*",
                r"jsr j_ClearFlag",
                r"moveq #(?P<value>\d+),d0",
                r"jsr \(Sleep\)\.w",
                r"rts",
            ],
            owner=handler_name,
        )
        sleep_match = re.fullmatch(r"moveq #(?P<value>\d+),d0", ordered[9])
        if sleep_match is None:
            raise ValueError("story-state yes/no sleep use shape drift")
        return {
            "orderedInstructions": ordered,
            "branchRecords": [
                {
                    "testInstruction": ordered[4],
                    "branchInstruction": ordered[5],
                    "fallthroughInstruction": ordered[6],
                    "branchTargetInstruction": ordered[8],
                }
            ],
            "sourceConstantUses": [
                {"constant": constant, "value": equates[constant], "instruction": ordered[3]}
            ],
            "sourceLiteralUses": [
                {"value": _literal(sleep_match.group("value")), "instruction": ordered[9]}
            ],
            "mutationCallOrder": [
                ordered[0],
                ordered[1],
                ordered[2],
                ordered[6],
                ordered[8],
                ordered[9],
                ordered[10],
            ],
        }
    if handler_name == "csc13_setStoryFlag":
        constant = "BATTLE_UNLOCKED_FLAGS_START"
        if constant not in equates:
            raise ValueError("story-state battle-unlock source constant is missing")
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w \(a6\)\+,d1",
                rf"addi\.w #{constant},d1",
                r"jsr j_SetFlag",
                r"rts",
            ],
            owner=handler_name,
        )
        return {
            "orderedInstructions": ordered,
            "branchRecords": [],
            "sourceConstantUses": [
                {"constant": constant, "value": equates[constant], "instruction": ordered[1]}
            ],
            "sourceLiteralUses": [],
            "mutationCallOrder": [ordered[1], ordered[2]],
        }
    raise ValueError(f"story-state handler guard has no profile: {handler_name}")


def _story_state_corpus_order_facts(
    source_sites: list[dict[str, Any]], program_totals: list[dict[str, Any]]
) -> dict[str, Any]:
    """Derive compact, schema-pinned order and content evidence for both corpora."""
    source_site_order_keys: list[str] = []
    for site in source_sites:
        for command in site["commands"]:
            reference = command["storyStateReference"]
            source_site_order_keys.append(
                ":".join(
                    (
                        site["programId"],
                        str(command["commandIndex"]),
                        command["macro"],
                        reference["field"],
                        str(reference["entryIndex"]),
                    )
                )
            )
    if len(source_site_order_keys) != len(set(source_site_order_keys)):
        raise ValueError("story-state source-site order keys are not unique")
    program_total_order_keys = [row["programId"] for row in program_totals]
    if len(program_total_order_keys) != len(set(program_total_order_keys)):
        raise ValueError("story-state program-total order keys are not unique")
    return {
        "sourceSiteOrderKeys": source_site_order_keys,
        "sourceSitesSha256": hashlib.sha256(
            _canonical_bytes({"sourceSites": source_sites})
        ).hexdigest().upper(),
        "programTotalOrderKeys": program_total_order_keys,
        "programTotalsSha256": hashlib.sha256(
            _canonical_bytes({"programTotals": program_totals})
        ).hexdigest().upper(),
    }


def _story_state_source_identity_joins(disasm: Path, upstream_path: Path) -> dict[str, Any]:
    """Record only the flag and yes/no service owners used by this handler group."""
    stats = build_stats_inventory(upstream_path)
    flags_path = "code/common/stats/gameflags.asm"
    flags = next((row for row in stats["files"] if row["path"] == flags_path), None)
    if flags is None:
        raise ValueError("story-state common-stats flags owner is missing")
    symbols = ["CheckFlag", "SetFlag", "ClearFlag"]
    if not set(symbols) <= set(flags["globalLabels"]):
        raise ValueError("story-state common-stats flags owner symbols drift")

    yes_no_path = "code/common/menus/yesnoprompt.asm"
    yes_no_source = read_upstream_text(disasm / yes_no_path)
    if re.search(r"^YesNoPrompt:\s*$", yes_no_source, re.MULTILINE) is None:
        raise ValueError("story-state yes/no owner symbol drift")
    return {
        "commonStatsFlags": {
            "contractId": stats["id"],
            "upstreamCommit": stats["upstream"]["commit"],
            "sourcePath": flags["path"],
            "sourceSha256": flags["sha256"],
            "symbols": symbols,
        },
        "yesNoOwner": {
            "sourcePath": yes_no_path,
            "sourceSha256": hashlib.sha256(yes_no_source.encode()).hexdigest().upper(),
            "symbols": ["YesNoPrompt"],
        },
    }


def _story_state_command_facts(
    disasm: Path,
    equates: dict[str, int],
    macros: dict[str, dict[str, Any]],
    dispatch_targets: list[str],
    handlers: list[dict[str, Any]],
    program_corpus: dict[str, Any],
    addresses: dict[str, int],
    rom: bytes,
    upstream_path: Path,
) -> dict[str, Any]:
    """Build the seven-form story-state branch/mutation static contract."""
    required_constants = ("FLAG_INDEX_YES_NO_PROMPT", "BATTLE_UNLOCKED_FLAGS_START")
    missing = [name for name in required_constants if name not in equates]
    if missing:
        raise ValueError(f"story-state source constants are missing: {missing}")
    source_sites, program_totals, story_state_cross_checks = _story_state_program_facts(
        program_corpus
    )
    corpus_order_facts = _story_state_corpus_order_facts(source_sites, program_totals)
    source_counts: Counter[str] = Counter()
    for site in source_sites:
        source_counts.update(command["macro"] for command in site["commands"])
    for macro in STORY_STATE_MACRO_NAMES:
        if sum(row["macroCounts"][macro] for row in program_totals) != source_counts[macro]:
            raise ValueError(f"story-state program total drift: {macro}")

    primary = macros["csc10"]
    for alias in ("setF", "clearF"):
        contract = macros[alias]
        if (
            contract["aliasOf"] != "csc10"
            or contract["encodedBytes"] != primary["encodedBytes"]
            or contract["operandBytes"] != primary["operandBytes"]
            or len(primary["operandLayout"]) != 2
            or len(contract["operandLayout"]) != len(primary["operandLayout"])
            or contract["operandLayout"][0] != primary["operandLayout"][0]
            or contract["operandLayout"][1]["streamOffset"]
            != primary["operandLayout"][1]["streamOffset"]
            or contract["operandLayout"][1]["widthBytes"]
            != primary["operandLayout"][1]["widthBytes"]
            or contract["operandLayout"][1]["parameterOrdinals"]
        ):
            raise ValueError(f"story-state csc10 alias layout drift: {alias}")
        fixed_value = _literal(contract["operandLayout"][1]["expression"])
        if (alias == "setF" and fixed_value == 0) or (
            alias == "clearF" and fixed_value != 0
        ):
            raise ValueError(f"story-state csc10 alias selector drift: {alias}")

    handler_rows = []
    direct_call_rows: dict[str, list[dict[str, str]]] = {}
    for handler_name in STORY_STATE_HANDLER_NAMES:
        handler_macros = [
            name
            for name in STORY_STATE_MACRO_NAMES
            if STORY_STATE_HANDLER_BY_MACRO[name] == handler_name
        ]
        handler = _handler_by_name(handlers, handler_name)
        opcode = macros[handler_macros[0]]["opcode"]
        if opcode is None or dispatch_targets[opcode] != handler_name:
            raise ValueError(f"story-state dispatcher target drift: {handler_name}")
        if handler["opcodes"] != [opcode]:
            raise ValueError(f"story-state handler opcode drift: {handler_name}")
        for macro in handler_macros:
            contract = macros[macro]
            if contract["kind"] != "command" or contract["opcode"] != opcode:
                raise ValueError(f"story-state macro ABI drift: {macro}")
            if handler["encodedCommandBytes"] != contract["encodedBytes"]:
                raise ValueError(f"story-state handler encoded width drift: {macro}")
        statements = _stable_handler_statements(disasm, handler)
        direct_calls = _force_state_direct_calls(statements)
        direct_call_rows[handler_name] = direct_calls
        handler_rows.append(
            {
                "handler": handler_name,
                "macros": handler_macros,
                "address": handler["address"],
                "opcode": opcode,
                "sourceCommandCount": sum(source_counts[name] for name in handler_macros),
                "cursorReadWidths": [
                    field["widthBytes"] for field in macros[handler_macros[0]]["operandLayout"]
                ],
                "statementCount": len(statements),
                "guardedStatements": statements,
                "sectionGuard": _story_state_section_guard(handler_name, statements, equates),
                "directCalls": direct_calls,
            }
        )
    for handler in handler_rows[:2]:
        pointer_width = macros[handler["macros"][0]]["operandLayout"][1]["widthBytes"]
        if handler["sectionGuard"]["sourceLiteralUses"][0]["value"] != pointer_width:
            raise ValueError(
                f"story-state conditional target skip width drift: {handler['handler']}"
            )
    return {
        "macros": [
            {
                "name": name,
                "opcode": macros[name]["opcode"],
                "encodedBytes": macros[name]["encodedBytes"],
                "operandBytes": macros[name]["operandBytes"],
                "operandLayout": macros[name]["operandLayout"],
                "parameterOrdinals": macros[name]["parameterOrdinals"],
                "aliasOf": macros[name]["aliasOf"],
                "handler": STORY_STATE_HANDLER_BY_MACRO[name],
                "sourceCommandCount": source_counts[name],
            }
            for name in STORY_STATE_MACRO_NAMES
        ],
        "sourceSites": source_sites,
        **corpus_order_facts,
        "programTotals": program_totals,
        "programCorpusReferences": story_state_cross_checks,
        "handlers": handler_rows,
        "callerBreakdown": _force_state_caller_breakdown(
            disasm,
            handlers,
            STORY_STATE_HANDLER_NAMES,
            direct_call_rows,
            addresses,
            rom,
        ),
        "sourceIdentityJoins": _story_state_source_identity_joins(disasm, upstream_path),
        "runtimeQuestions": STORY_STATE_RUNTIME_QUESTIONS,
    }


def _map_block_macro_operand_fields(disasm: Path) -> dict[str, list[dict[str, Any]]]:
    """Parse the source comments and byte fields of the two bounded macro forms."""
    blocks = _macro_blocks(read_upstream_text(disasm / MACRO_PATH))
    fields_by_macro: dict[str, list[dict[str, Any]]] = {}
    for macro in MAP_BLOCK_MUTATION_MACRO_NAMES:
        body = blocks.get(macro)
        if body is None:
            raise ValueError(f"map-block mutation macro is missing: {macro}")
        emission_rows = _emission_rows(body)
        parameter_rows = [row for row in emission_rows if row["parameterOrdinals"]]
        fields: list[dict[str, Any]] = []
        for raw_line in body.splitlines():
            match = re.fullmatch(
                r"\s*dc\.b\s+\\(?P<ordinal>\d+)\s*;\s*(?P<label>[^;]+?)\s*",
                raw_line,
            )
            if match is None:
                continue
            if len(fields) >= len(parameter_rows):
                raise ValueError(f"map-block mutation operand emission drift: {macro}")
            row = parameter_rows[len(fields)]
            if row["parameterOrdinals"] != [int(match.group("ordinal"))]:
                raise ValueError(f"map-block mutation operand ordinal drift: {macro}")
            fields.append(
                {
                    "parameterOrdinal": int(match.group("ordinal")),
                    "sourceLabel": match.group("label").strip(),
                    "streamOffset": row["streamOffset"],
                    "widthBytes": row["widthBytes"],
                }
            )
        if not fields:
            raise ValueError(f"map-block mutation operand fields are missing: {macro}")
        if len(fields) != len(parameter_rows):
            raise ValueError(f"map-block mutation operand comment coverage drift: {macro}")
        if [field["parameterOrdinal"] for field in fields] != list(
            range(1, len(fields) + 1)
        ):
            raise ValueError(f"map-block mutation operand ordinal drift: {macro}")
        fields_by_macro[macro] = fields
    if fields_by_macro["setBlocks"] != fields_by_macro["setBlocksVar"]:
        raise ValueError("map-block mutation macro field layout disagreement")
    return fields_by_macro


def _map_block_mutation_program_facts(
    program_corpus: dict[str, Any], operand_fields: dict[str, list[dict[str, Any]]]
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    """Attach parsed byte operand records to all source-shaped mutation sites."""
    source_sites, program_totals = _force_state_program_facts(
        program_corpus, macro_names=MAP_BLOCK_MUTATION_MACRO_NAMES
    )
    annotated_sites: list[dict[str, Any]] = []
    for site in source_sites:
        commands: list[dict[str, Any]] = []
        for command in site["commands"]:
            fields = operand_fields[command["macro"]]
            if len(command["arguments"]) != len(fields):
                raise ValueError(
                    "map-block mutation source operand count drift: "
                    f"{site['programId']}:{command['commandIndex']}"
                )
            order_key = ":".join(
                (site["programId"], str(command["commandIndex"]), command["macro"])
            )
            commands.append(
                {
                    **command,
                    "sourceOrderKey": order_key,
                    "operandValues": [
                        {
                            "parameterOrdinal": field["parameterOrdinal"],
                            "sourceLabel": field["sourceLabel"],
                            "rawValue": raw_value,
                            "value": _literal(raw_value),
                        }
                        for field, raw_value in zip(fields, command["arguments"], strict=True)
                    ],
                }
            )
        annotated_sites.append({"programId": site["programId"], "commands": commands})
    return annotated_sites, program_totals


def _map_block_mutation_corpus_order_facts(
    source_sites: list[dict[str, Any]], program_totals: list[dict[str, Any]]
) -> dict[str, Any]:
    """Derive compact ordering and content checks for the two large source corpora."""
    source_order_keys = [
        command["sourceOrderKey"]
        for site in source_sites
        for command in site["commands"]
    ]
    if len(source_order_keys) != len(set(source_order_keys)):
        raise ValueError("map-block mutation source order keys are not unique")
    program_total_order_keys = [row["programId"] for row in program_totals]
    if len(program_total_order_keys) != len(set(program_total_order_keys)):
        raise ValueError("map-block mutation program-total order keys are not unique")
    return {
        "sourceSiteOrderKeys": source_order_keys,
        "sourceSitesSha256": hashlib.sha256(
            _canonical_bytes({"sourceSites": source_sites})
        ).hexdigest().upper(),
        "programTotalOrderKeys": program_total_order_keys,
        "programTotalsSha256": hashlib.sha256(
            _canonical_bytes({"programTotals": program_totals})
        ).hexdigest().upper(),
    }


def _map_block_mutation_section_guard(
    handler_name: str, statements: list[str]
) -> dict[str, Any]:
    """Guard the complete cursor/call/bit-set order of one mutation handler."""
    common_patterns = [
        r"move\.w \(a6\)\+,d0",
        r"move\.w \(a6\)\+,d1",
        r"move\.w \(a6\)\+,d2",
        r"jsr \(CopyMapBlocks\)\.w",
    ]
    if handler_name == "csc34_setBlocks":
        ordered = _force_state_ordered_statements(
            statements,
            [
                *common_patterns,
                r"bset #(?P<bit>\d+),\(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD\)\.l",
                r"bset #(?P<bit>\d+),\(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD\)\.l",
                r"rts",
            ],
            owner=handler_name,
        )
        bit_set_uses = []
        for instruction in ordered[4:6]:
            match = re.fullmatch(
                r"bset #(?P<bit>\d+),\(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD\)\.l",
                instruction,
            )
            if match is None:
                raise ValueError("map-block mutation bit-set use shape drift")
            bit_set_uses.append(
                {
                    "bitIndex": _literal(match.group("bit")),
                    "sourceTarget": "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
                    "instruction": instruction,
                }
            )
        if bit_set_uses[1]["bitIndex"] != bit_set_uses[0]["bitIndex"] + 1:
            raise ValueError("map-block mutation bit-set order drift")
    elif handler_name == "csc35_setBlocksVar":
        ordered = _force_state_ordered_statements(
            statements, [*common_patterns, r"rts"], owner=handler_name
        )
        bit_set_uses = []
    else:
        raise ValueError(f"map-block mutation handler guard has no profile: {handler_name}")
    if len(statements) != len(ordered):
        raise ValueError(f"map-block mutation handler statement coverage drift: {handler_name}")
    cursor_reads = []
    width_by_size_suffix = {"b": 1, "w": 2, "l": 4}
    for instruction in ordered[:3]:
        match = re.fullmatch(
            r"move\.(?P<size>[bwl]) \(a6\)\+,(?P<register>d[0-7])", instruction
        )
        if match is None:
            raise ValueError("map-block mutation cursor read shape drift")
        cursor_reads.append(
            {
                "handlerRegister": match.group("register"),
                "transferredByteCount": width_by_size_suffix[match.group("size")],
                "instruction": instruction,
            }
        )
    return {
        "orderedInstructions": ordered,
        "cursorReadUseSites": cursor_reads,
        "directCallOrder": [ordered[3]],
        "postCallBitSetUseSites": bit_set_uses,
        "returnInstruction": ordered[-1],
    }


def _map_block_input_word_groups(
    operand_fields: list[dict[str, Any]], section_guard: dict[str, Any]
) -> list[dict[str, Any]]:
    """Link byte-emitting macro operands to the parsed A6 word-read use sites."""
    groups: list[dict[str, Any]] = []
    field_index = 0
    for read in section_guard["cursorReadUseSites"]:
        start = field_index
        remaining = read["transferredByteCount"]
        while remaining:
            if field_index >= len(operand_fields):
                raise ValueError("map-block mutation cursor reads exceed macro fields")
            field = operand_fields[field_index]
            if field["widthBytes"] > remaining:
                raise ValueError("map-block mutation cursor read splits one macro field")
            remaining -= field["widthBytes"]
            field_index += 1
        fields = operand_fields[start:field_index]
        groups.append(
            {
                "handlerRegister": read["handlerRegister"],
                "sourceParameterOrdinals": [field["parameterOrdinal"] for field in fields],
                "sourceLabels": [field["sourceLabel"] for field in fields],
                "streamOffset": fields[0]["streamOffset"],
                "transferredByteCount": read["transferredByteCount"],
                "cursorReadInstruction": read["instruction"],
            }
        )
    if field_index != len(operand_fields):
        raise ValueError("map-block mutation macro fields exceed cursor reads")
    return groups


def _map_block_named_section_statements(
    disasm: Path, source_path: str, name: str
) -> list[str]:
    """Read the bounded helper section without relying on a file-wide fragment."""
    source = read_upstream_text(disasm / source_path)
    match = re.search(
        rf"^{re.escape(name)}:\s*\n(?P<body>.*?)"
        rf"^\s*; End of function {re.escape(name)}\s*$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if match is None:
        raise ValueError(f"map-block mutation helper section is missing: {name}")
    return _statements(match.group("body"))


def _map_block_copy_helper_facts(
    disasm: Path, equates: dict[str, int], addresses: dict[str, int]
) -> dict[str, Any]:
    """Parse source use sites in the called helper without assigning game-level effects."""
    constant = "BYTE_SHIFT_COUNT"
    if constant not in equates:
        raise ValueError("map-block mutation byte-shift constant is missing")
    helper = {
        "name": "CopyMapBlocks",
        "sourcePath": "code/gameflow/exploration/exploration.asm",
    }
    if helper["name"] not in addresses:
        raise ValueError("map-block mutation copy helper lacks an H1 address")
    statements = _map_block_named_section_statements(
        disasm, helper["sourcePath"], helper["name"]
    )
    ordered = _force_state_ordered_statements(
        statements,
        [
            r"movem\.l d0-d7/a2,-\(sp\)",
            r"clr\.w d7",
            r"move\.b d1,d7",
            r"subq\.w #1,d7",
            r"move\.w d1,d6",
            rf"lsr\.w #{constant},d6",
            r"subq\.w #1,d6",
            r"clr\.w d3",
            r"move\.b d2,d3",
            rf"lsr\.w #{constant},d2",
            r"lsl\.w #(?P<bits>\d+),d3",
            r"add\.w d3,d2",
            r"add\.w d2,d2",
            r"clr\.w d1",
            r"move\.b d0,d1",
            rf"lsr\.w #{constant},d0",
            r"lsl\.w #(?P<bits>\d+),d1",
            r"add\.w d1,d0",
            r"add\.w d0,d0",
            r"lea \(FF0000_RAM_START\)\.l,a2",
            r"movem\.w d0/d2/d6,-\(sp\)",
            r"move\.w \(a2,d0\.w\),\(a2,d2\.w\)",
            r"addq\.w #(?P<stride>\d+),d0",
            r"addq\.w #(?P<stride>\d+),d2",
            r"dbf d6,[A-Za-z_][A-Za-z0-9_]*",
            r"movem\.w \(sp\)\+,d0/d2/d6",
            r"addi\.w #(?P<stride>\d+),d0",
            r"addi\.w #(?P<stride>\d+),d2",
            r"dbf d7,[A-Za-z_][A-Za-z0-9_]*",
            r"movem\.l \(sp\)\+,d0-d7/a2",
            r"rts",
        ],
        owner="CopyMapBlocks",
    )
    if len(statements) != len(ordered):
        raise ValueError("map-block mutation copy-helper statement coverage drift")
    shift_matches = [
        re.fullmatch(r"lsl\.w #(?P<value>\d+),d[13]", ordered[index])
        for index in (10, 16)
    ]
    word_stride_matches = [
        re.fullmatch(r"addq\.w #(?P<value>\d+),d[02]", ordered[index])
        for index in (22, 23)
    ]
    row_stride_matches = [
        re.fullmatch(r"addi\.w #(?P<value>\d+),d[02]", ordered[index])
        for index in (26, 27)
    ]
    if any(match is None for match in (*shift_matches, *word_stride_matches, *row_stride_matches)):
        raise ValueError("map-block mutation copy-helper literal use shape drift")
    row_shift_bits = _literal(shift_matches[0].group("value"))
    word_copy_byte_stride = _literal(word_stride_matches[0].group("value"))
    row_byte_stride = _literal(row_stride_matches[0].group("value"))
    if (
        any(_literal(match.group("value")) != row_shift_bits for match in shift_matches)
        or any(
            _literal(match.group("value")) != word_copy_byte_stride
            for match in word_stride_matches
        )
        or any(_literal(match.group("value")) != row_byte_stride for match in row_stride_matches)
    ):
        raise ValueError("map-block mutation copy-helper paired literal disagreement")
    if row_byte_stride != word_copy_byte_stride * (1 << row_shift_bits):
        raise ValueError("map-block mutation copy-helper row-stride relationship drift")
    return {
        "helper": helper["name"],
        "address": addresses[helper["name"]],
        "orderedInstructions": ordered,
        "inputByteShiftConstantUses": [
            {"constant": constant, "value": equates[constant], "instruction": ordered[index]}
            for index in (5, 9, 15)
        ],
        "addressRowShiftUses": [
            {"value": row_shift_bits, "instruction": ordered[index]} for index in (10, 16)
        ],
        "wordCopyByteStrideUses": [
            {"value": word_copy_byte_stride, "instruction": ordered[index]}
            for index in (22, 23)
        ],
        "rowByteStrideUses": [
            {"value": row_byte_stride, "instruction": ordered[index]}
            for index in (26, 27)
        ],
        "copyInstruction": ordered[21],
        "innerLoop": {
            "counterRegister": "d6",
            "seedInstruction": ordered[4],
            "decrementInstruction": ordered[6],
            "loopInstruction": ordered[24],
        },
        "outerLoop": {
            "counterRegister": "d7",
            "seedInstruction": ordered[2],
            "decrementInstruction": ordered[3],
            "loopInstruction": ordered[28],
        },
        "derivedAddressStride": {
            "addressRowShiftBits": row_shift_bits,
            "wordCopyByteStride": word_copy_byte_stride,
            "rowByteStride": row_byte_stride,
        },
    }


def _map_block_mutation_source_identity_joins(disasm: Path) -> dict[str, Any]:
    """Record just the source owner of the direct helper call."""
    source_path = "code/gameflow/exploration/exploration.asm"
    source = read_upstream_text(disasm / source_path)
    if re.search(r"^CopyMapBlocks:\s*$", source, re.MULTILINE) is None:
        raise ValueError("map-block mutation copy helper owner symbol drift")
    return {
        "copyMapBlocksOwner": {
            "sourcePath": source_path,
            "sourceSha256": hashlib.sha256(source.encode()).hexdigest().upper(),
            "symbols": ["CopyMapBlocks"],
        }
    }


def _map_block_mutation_operand_value_bounds(
    source_sites: list[dict[str, Any]], operand_fields: list[dict[str, Any]]
) -> list[dict[str, Any]]:
    """Summarize each parsed source operand with source-site provenance for its bounds."""
    values_by_ordinal: dict[int, list[tuple[int, str]]] = {
        field["parameterOrdinal"]: [] for field in operand_fields
    }
    for site in source_sites:
        for command in site["commands"]:
            for operand in command["operandValues"]:
                values_by_ordinal[operand["parameterOrdinal"]].append(
                    (operand["value"], command["sourceOrderKey"])
                )
    rows = []
    for field in operand_fields:
        values = values_by_ordinal[field["parameterOrdinal"]]
        if not values:
            raise ValueError("map-block mutation operand has no source values")
        minimum = min(values)
        maximum = max(values)
        rows.append(
            {
                "parameterOrdinal": field["parameterOrdinal"],
                "sourceLabel": field["sourceLabel"],
                "minimumValue": minimum[0],
                "minimumSourceSiteKey": minimum[1],
                "maximumValue": maximum[0],
                "maximumSourceSiteKey": maximum[1],
                "sourceValueCount": len(values),
            }
        )
    return rows


def _map_block_mutation_command_facts(
    disasm: Path,
    equates: dict[str, int],
    macros: dict[str, dict[str, Any]],
    dispatch_targets: list[str],
    handlers: list[dict[str, Any]],
    program_corpus: dict[str, Any],
    addresses: dict[str, int],
    rom: bytes,
) -> dict[str, Any]:
    """Build the source-faithful static contract for map block mutation commands."""
    operand_fields_by_macro = _map_block_macro_operand_fields(disasm)
    source_sites, program_totals = _map_block_mutation_program_facts(
        program_corpus, operand_fields_by_macro
    )
    corpus_order_facts = _map_block_mutation_corpus_order_facts(source_sites, program_totals)
    source_counts: Counter[str] = Counter(
        command["macro"] for site in source_sites for command in site["commands"]
    )
    for macro in MAP_BLOCK_MUTATION_MACRO_NAMES:
        if sum(row["macroCounts"][macro] for row in program_totals) != source_counts[macro]:
            raise ValueError(f"map-block mutation program total drift: {macro}")
        contract = macros[macro]
        fields = operand_fields_by_macro[macro]
        source_operand_bytes = sum(field["widthBytes"] for field in fields)
        source_encoded_bytes = max(
            field["streamOffset"] + field["widthBytes"] for field in fields
        )
        if (
            contract["kind"] != "command"
            or contract["opcode"] is None
            or contract["operandBytes"] != source_operand_bytes
            or contract["encodedBytes"] != source_encoded_bytes
            or contract["operandLayout"] != [
                {
                    "streamOffset": field["streamOffset"],
                    "widthBytes": field["widthBytes"],
                    "expression": f"\\{field['parameterOrdinal']}",
                    "parameterOrdinals": [field["parameterOrdinal"]],
                    "encoding": "direct",
                }
                for field in fields
            ]
        ):
            raise ValueError(f"map-block mutation macro operand layout drift: {macro}")

    handler_rows = []
    direct_call_rows: dict[str, list[dict[str, str]]] = {}
    primary_guard: dict[str, Any] | None = None
    for handler_name in MAP_BLOCK_MUTATION_HANDLER_NAMES:
        handler_macros = [
            macro
            for macro in MAP_BLOCK_MUTATION_MACRO_NAMES
            if MAP_BLOCK_MUTATION_HANDLER_BY_MACRO[macro] == handler_name
        ]
        handler = _handler_by_name(handlers, handler_name)
        opcode = macros[handler_macros[0]]["opcode"]
        if opcode is None or dispatch_targets[opcode] != handler_name:
            raise ValueError(f"map-block mutation dispatcher target drift: {handler_name}")
        if handler["opcodes"] != [opcode]:
            raise ValueError(f"map-block mutation handler opcode drift: {handler_name}")
        for macro in handler_macros:
            contract = macros[macro]
            if (
                contract["kind"] != "command"
                or contract["opcode"] != opcode
                or handler["encodedCommandBytes"] != contract["encodedBytes"]
            ):
                raise ValueError(f"map-block mutation handler ABI drift: {macro}")
        statements = _stable_handler_statements(disasm, handler)
        section_guard = _map_block_mutation_section_guard(handler_name, statements)
        direct_calls = _force_state_direct_calls(statements)
        if direct_calls != [{"opcode": "jsr", "instructionTarget": "CopyMapBlocks"}]:
            raise ValueError(f"map-block mutation direct-call inventory drift: {handler_name}")
        direct_call_rows[handler_name] = direct_calls
        if primary_guard is None:
            primary_guard = section_guard
        elif section_guard["cursorReadUseSites"] != primary_guard["cursorReadUseSites"]:
            raise ValueError("map-block mutation handler cursor-read disagreement")
        handler_rows.append(
            {
                "handler": handler_name,
                "macros": handler_macros,
                "address": handler["address"],
                "opcode": opcode,
                "sourceCommandCount": sum(source_counts[macro] for macro in handler_macros),
                "cursorReadWidths": [
                    read["transferredByteCount"] for read in section_guard["cursorReadUseSites"]
                ],
                "statementCount": len(statements),
                "guardedStatements": statements,
                "sectionGuard": section_guard,
                "directCalls": direct_calls,
            }
        )
    if primary_guard is None:
        raise ValueError("map-block mutation primary handler guard is missing")
    input_word_groups = _map_block_input_word_groups(
        operand_fields_by_macro["setBlocks"], primary_guard
    )
    return {
        "macros": [
            {
                "name": macro,
                "opcode": macros[macro]["opcode"],
                "encodedBytes": macros[macro]["encodedBytes"],
                "operandBytes": macros[macro]["operandBytes"],
                "operandLayout": macros[macro]["operandLayout"],
                "parameterOrdinals": macros[macro]["parameterOrdinals"],
                "handler": MAP_BLOCK_MUTATION_HANDLER_BY_MACRO[macro],
                "sourceOperandFields": operand_fields_by_macro[macro],
                "sourceCommandCount": source_counts[macro],
            }
            for macro in MAP_BLOCK_MUTATION_MACRO_NAMES
        ],
        "sourceSites": source_sites,
        **corpus_order_facts,
        "programTotals": program_totals,
        "operandValueBounds": _map_block_mutation_operand_value_bounds(
            source_sites, operand_fields_by_macro["setBlocks"]
        ),
        "inputWordGroups": input_word_groups,
        "handlers": handler_rows,
        "copyMapBlocksHelperFacts": _map_block_copy_helper_facts(disasm, equates, addresses),
        "callerBreakdown": _force_state_caller_breakdown(
            disasm,
            handlers,
            MAP_BLOCK_MUTATION_HANDLER_NAMES,
            direct_call_rows,
            addresses,
            rom,
        ),
        "sourceIdentityJoins": _map_block_mutation_source_identity_joins(disasm),
        "runtimeQuestions": MAP_BLOCK_MUTATION_RUNTIME_QUESTIONS,
    }


def _entity_population_macro_annotations(disasm: Path) -> dict[str, list[dict[str, Any]]]:
    """Parse byte widths, offsets, and source comments for the bounded macro forms."""
    blocks = _macro_blocks(read_upstream_text(disasm / MACRO_PATH))
    annotations_by_macro: dict[str, list[dict[str, Any]]] = {}
    for macro in ENTITY_POPULATION_MACRO_NAMES:
        body = blocks.get(macro)
        if body is None:
            raise ValueError(f"entity-population macro is missing: {macro}")
        parameter_rows = [row for row in _emission_rows(body) if row["parameterOrdinals"]]
        annotations: list[dict[str, Any]] = []
        for raw_line in body.splitlines():
            match = re.fullmatch(
                r"\s*dc\.[bwl]\s+\\(?P<ordinal>\d+)"
                r"(?:\s*;\s*(?P<comment>.*))?\s*",
                raw_line,
            )
            if match is None:
                continue
            if len(annotations) >= len(parameter_rows):
                raise ValueError(f"entity-population operand emission drift: {macro}")
            row = parameter_rows[len(annotations)]
            ordinal = int(match.group("ordinal"))
            if row["parameterOrdinals"] != [ordinal]:
                raise ValueError(f"entity-population operand ordinal drift: {macro}")
            comment = match.group("comment")
            if comment is None:
                raise ValueError(f"entity-population operand comment is missing: {macro}")
            annotations.append(
                {
                    "parameterOrdinal": ordinal,
                    "sourceComment": comment,
                    "streamOffset": row["streamOffset"],
                    "widthBytes": row["widthBytes"],
                }
            )
        if len(annotations) != len(parameter_rows):
            raise ValueError(f"entity-population operand comment coverage drift: {macro}")
        if [row["parameterOrdinal"] for row in annotations] != list(
            range(1, len(annotations) + 1)
        ):
            raise ValueError(f"entity-population operand ordinal sequence drift: {macro}")
        annotations_by_macro[macro] = annotations
    return annotations_by_macro


def _entity_population_resolve_operand(value: str, equates: dict[str, int]) -> dict[str, Any]:
    """Keep source operands distinct when they are numeric equates versus symbols."""
    token = value.strip()
    if token in equates:
        return {"rawValue": value, "resolvedValue": equates[token], "resolution": "equate"}
    try:
        return {"rawValue": value, "resolvedValue": _literal(token), "resolution": "literal"}
    except ValueError:
        return {"rawValue": value, "resolvedValue": None, "resolution": "symbol"}


def _entity_population_program_facts(
    program_corpus: dict[str, Any],
    annotations_by_macro: dict[str, list[dict[str, Any]]],
    equates: dict[str, int],
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    """Attach source-faithful operand records to each bounded command occurrence."""
    source_sites, program_totals = _force_state_program_facts(
        program_corpus, macro_names=ENTITY_POPULATION_MACRO_NAMES
    )
    annotated_sites: list[dict[str, Any]] = []
    for site in source_sites:
        commands = []
        for command in site["commands"]:
            annotations = annotations_by_macro[command["macro"]]
            if len(command["arguments"]) != len(annotations):
                raise ValueError(
                    "entity-population source operand count drift: "
                    f"{site['programId']}:{command['commandIndex']}"
                )
            source_order_key = ":".join(
                (site["programId"], str(command["commandIndex"]), command["macro"])
            )
            commands.append(
                {
                    **command,
                    "sourceOrderKey": source_order_key,
                    "operandValues": [
                        {
                            "parameterOrdinal": annotation["parameterOrdinal"],
                            "sourceComment": annotation["sourceComment"],
                            "streamOffset": annotation["streamOffset"],
                            "widthBytes": annotation["widthBytes"],
                            **_entity_population_resolve_operand(argument, equates),
                        }
                        for annotation, argument in zip(
                            annotations, command["arguments"], strict=True
                        )
                    ],
                }
            )
        annotated_sites.append({"programId": site["programId"], "commands": commands})
    return annotated_sites, program_totals


def _entity_population_corpus_order_facts(
    source_sites: list[dict[str, Any]], program_totals: list[dict[str, Any]]
) -> dict[str, Any]:
    """Bind the complete source-command and zero-inclusive program corpus compactly."""
    source_order_keys = [
        command["sourceOrderKey"] for site in source_sites for command in site["commands"]
    ]
    if len(source_order_keys) != len(set(source_order_keys)):
        raise ValueError("entity-population source order keys are not unique")
    program_order_keys = [row["programId"] for row in program_totals]
    if len(program_order_keys) != len(set(program_order_keys)):
        raise ValueError("entity-population program-total order keys are not unique")
    return {
        "sourceSiteOrderKeys": source_order_keys,
        "sourceSitesSha256": hashlib.sha256(
            _canonical_bytes({"sourceSites": source_sites})
        ).hexdigest().upper(),
        "programTotalOrderKeys": program_order_keys,
        "programTotalsSha256": hashlib.sha256(
            _canonical_bytes({"programTotals": program_totals})
        ).hexdigest().upper(),
    }


def _entity_population_read_use_site(instruction: str) -> dict[str, Any]:
    """Parse a bounded source-pointer read with its instruction size suffix."""
    match = re.fullmatch(
        r"move(?P<address>a)?\.(?P<size>[bwl]) \((?P<source>a[06])\)\+,(?P<target>[ad][0-7])",
        instruction,
    )
    if match is None:
        raise ValueError("entity-population pointer-read use shape drift")
    is_address_move = match.group("address") is not None
    target = match.group("target")
    if is_address_move != target.startswith("a") or (
        is_address_move and match.group("size") != "l"
    ):
        raise ValueError("entity-population pointer-read use shape drift")
    return {
        "sourceRegister": match.group("source"),
        "destinationRegister": target,
        "transferredByteCount": {"b": 1, "w": 2, "l": 4}[match.group("size")],
        "instruction": instruction,
    }


def _entity_population_vint_record(
    statements: list[str], index: int, phase: str, equates: dict[str, int]
) -> dict[str, Any]:
    """Tie each VInt phase name to its adjacent trap data use sites."""
    expected_symbol = "VINTS_DEACTIVATE" if phase == "deactivate" else "VINTS_ACTIVATE"
    expected = ["trap #VINT_FUNCTIONS", f"dc.w {expected_symbol}", "dc.l 0"]
    if statements[index : index + len(expected)] != expected:
        raise ValueError(f"entity-population VInt {phase} use-site order drift")
    if expected_symbol not in equates:
        raise ValueError(f"entity-population VInt source constant is missing: {expected_symbol}")
    return {
        "phase": phase,
        "operationSymbol": expected_symbol,
        "operationValue": equates[expected_symbol],
        "trapInstruction": statements[index],
        "operationInstruction": statements[index + 1],
        "argumentInstruction": statements[index + 2],
    }


def _entity_population_section_guard(
    macro: str, statements: list[str], equates: dict[str, int], addresses: dict[str, int]
) -> dict[str, Any]:
    """Guard the complete named-handler instruction shape for one source form."""
    if macro == "newEntity":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.w \(a6\)\+,d0",
                r"clr\.w d1",
                r"clr\.w d2",
                r"clr\.w d3",
                r"clr\.w d4",
                r"move\.b \(a6\)\+,d1",
                r"move\.b \(a6\)\+,d2",
                r"move\.b \(a6\)\+,d3",
                r"move\.b \(a6\)\+,d4",
                r"move\.l #eas_Init,d5",
                r"jsr InitializeNewEntity",
                r"rts",
            ],
            owner="csc2B_initializeNewEntity",
        )
        cursor_indexes = (0, 5, 6, 7, 8)
        pointer_indexes: tuple[int, ...] = ()
        vint_records = []
        source_constant_uses = [
            {
                "symbol": "eas_Init",
                "value": addresses["eas_Init"],
                "instruction": ordered[9],
            }
        ]
        direct_call_order = [ordered[10]]
    elif macro == "loadMapEntities":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"trap #VINT_FUNCTIONS",
                r"dc\.w VINTS_DEACTIVATE",
                r"dc\.l 0",
                r"jsr \(DisableDisplayAndInterrupts\)\.w",
                r"movea\.l \(a6\)\+,a0",
                r"move\.w \(a0\)\+,d1",
                r"move\.w \(a0\)\+,d2",
                r"move\.w \(a0\)\+,d3",
                r"jsr InitializeMapEntities",
                r"jsr \(LoadEntityMapsprites\)\.w",
                r"jsr \(EnableDisplayAndInterrupts\)\.w",
                r"trap #VINT_FUNCTIONS",
                r"dc\.w VINTS_ACTIVATE",
                r"dc\.l 0",
                r"rts",
            ],
            owner="csc42_loadMapEntities",
        )
        cursor_indexes = (4,)
        pointer_indexes = (5, 6, 7)
        vint_records = [
            _entity_population_vint_record(ordered, 0, "deactivate", equates),
            _entity_population_vint_record(ordered, 11, "activate", equates),
        ]
        source_constant_uses = []
        direct_call_order = [ordered[index] for index in (3, 8, 9, 10)]
    elif macro == "reloadEntities":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"trap #VINT_FUNCTIONS",
                r"dc\.w VINTS_DEACTIVATE",
                r"dc\.l 0",
                r"moveq #0,d0",
                r"bsr\.w GetEntityAddressFromCharacter",
                r"moveq #0,d1",
                r"move\.w \(a5\),d1",
                r"divu\.w #MAP_TILE_SIZE,d1",
                r"moveq #0,d2",
                r"move\.w ENTITYDEF_OFFSET_Y\(a5\),d2",
                r"divu\.w #MAP_TILE_SIZE,d2",
                r"clr\.w d3",
                r"move\.b ENTITYDEF_OFFSET_FACING\(a5\),d3",
                r"movea\.l \(a6\)\+,a0",
                r"jsr InitializeMapEntities",
                r"trap #VINT_FUNCTIONS",
                r"dc\.w VINTS_ACTIVATE",
                r"dc\.l 0",
                r"rts",
            ],
            owner="csc44_reloadEntities",
        )
        cursor_indexes = (13,)
        pointer_indexes = ()
        vint_records = [
            _entity_population_vint_record(ordered, 0, "deactivate", equates),
            _entity_population_vint_record(ordered, 15, "activate", equates),
        ]
        required_symbols = ("MAP_TILE_SIZE", "ENTITYDEF_OFFSET_Y", "ENTITYDEF_OFFSET_FACING")
        if any(symbol not in equates for symbol in required_symbols):
            raise ValueError("entity-population reload source constants are missing")
        source_constant_uses = [
            {
                "symbol": "MAP_TILE_SIZE",
                "value": equates["MAP_TILE_SIZE"],
                "instruction": ordered[7],
            },
            {
                "symbol": "ENTITYDEF_OFFSET_Y",
                "value": equates["ENTITYDEF_OFFSET_Y"],
                "instruction": ordered[9],
            },
            {
                "symbol": "MAP_TILE_SIZE",
                "value": equates["MAP_TILE_SIZE"],
                "instruction": ordered[10],
            },
            {
                "symbol": "ENTITYDEF_OFFSET_FACING",
                "value": equates["ENTITYDEF_OFFSET_FACING"],
                "instruction": ordered[12],
            },
        ]
        direct_call_order = [ordered[index] for index in (4, 14)]
    elif macro == "loadEntitiesFromMapSetup":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"trap #VINT_FUNCTIONS",
                r"dc\.w VINTS_DEACTIVATE",
                r"dc\.l 0",
                r"jsr \(DisableDisplayAndInterrupts\)\.w",
                r"jsr GetMapSetupEntityList",
                r"move\.w \(a6\)\+,d1",
                r"move\.w \(a6\)\+,d2",
                r"move\.w \(a6\)\+,d3",
                r"jsr j_InitializeMapEntities",
                r"jsr \(LoadEntityMapsprites\)\.w",
                r"jsr \(EnableDisplayAndInterrupts\)\.w",
                r"trap #VINT_FUNCTIONS",
                r"dc\.w VINTS_ACTIVATE",
                r"dc\.l 0",
                r"rts",
            ],
            owner="csc49_loadEntitiesFromMapSetup",
        )
        cursor_indexes = (5, 6, 7)
        pointer_indexes = ()
        vint_records = [
            _entity_population_vint_record(ordered, 0, "deactivate", equates),
            _entity_population_vint_record(ordered, 11, "activate", equates),
        ]
        source_constant_uses = []
        direct_call_order = [ordered[index] for index in (3, 4, 8, 9, 10)]
    else:
        raise ValueError(f"entity-population guard has no macro profile: {macro}")
    if len(statements) != len(ordered):
        raise ValueError(f"entity-population handler statement coverage drift: {macro}")
    return {
        "orderedInstructions": ordered,
        "scriptCursorReadUseSites": [
            _entity_population_read_use_site(ordered[index]) for index in cursor_indexes
        ],
        "pointerReadUseSites": [
            _entity_population_read_use_site(ordered[index]) for index in pointer_indexes
        ],
        "vintControlRecords": vint_records,
        "sourceConstantUses": source_constant_uses,
        "directCallOrder": direct_call_order,
        "returnInstruction": ordered[-1],
    }


def _entity_population_caller_breakdown(
    disasm: Path,
    handlers: list[dict[str, Any]],
    direct_call_rows: dict[str, list[dict[str, str]]],
    addresses: dict[str, int],
    rom: bytes,
) -> dict[str, Any]:
    """Retain instruction aliases even when several identities share one effective target."""
    instruction_targets = sorted(
        {call["instructionTarget"] for rows in direct_call_rows.values() for call in rows}
    )
    aliases = _force_state_aliases(disasm, set(instruction_targets), addresses, rom)
    bounded_handlers = set(ENTITY_POPULATION_HANDLER_NAMES)
    target_resolutions = []
    for target in instruction_targets:
        effective_target = aliases.get(target, {}).get("effectiveTarget", target)
        target_resolutions.append(
            {
                "instructionTarget": target,
                "effectiveTarget": effective_target,
                "aliasSourcePath": aliases.get(target, {}).get("sourcePath"),
                "effectiveTargetScope": (
                    "internal" if effective_target in bounded_handlers else "external"
                ),
            }
        )
    effective_targets = sorted({row["effectiveTarget"] for row in target_resolutions})
    resolution_by_instruction = {
        row["instructionTarget"]: row["effectiveTarget"] for row in target_resolutions
    }
    caller_rows = []
    for handler_name in ENTITY_POPULATION_HANDLER_NAMES:
        instruction_counts = {target: 0 for target in instruction_targets}
        effective_counts = {target: 0 for target in effective_targets}
        for call in direct_call_rows[handler_name]:
            instruction_counts[call["instructionTarget"]] += 1
            effective_counts[resolution_by_instruction[call["instructionTarget"]]] += 1
        caller_rows.append(
            {
                "handler": handler_name,
                "instructionTargetSiteCounts": instruction_counts,
                "effectiveTargetSiteCounts": effective_counts,
            }
        )
    instruction_totals = {
        target: sum(row["instructionTargetSiteCounts"][target] for row in caller_rows)
        for target in instruction_targets
    }
    effective_totals = {
        target: sum(row["effectiveTargetSiteCounts"][target] for row in caller_rows)
        for target in effective_targets
    }
    scopes_by_effective_target: dict[str, str] = {}
    for row in target_resolutions:
        current = scopes_by_effective_target.setdefault(
            row["effectiveTarget"], row["effectiveTargetScope"]
        )
        if current != row["effectiveTargetScope"]:
            raise ValueError("entity-population effective target scope disagreement")

    def scoped_totals(scope: str) -> dict[str, int]:
        return {
            target: effective_totals[target]
            if scopes_by_effective_target[target] == scope
            else 0
            for target in effective_targets
        }

    return {
        "callerHandlers": caller_rows,
        "targetResolutions": target_resolutions,
        "instructionTargetTotals": instruction_totals,
        "effectiveTargetTotals": effective_totals,
        "internalEffectiveTargetTotals": scoped_totals("internal"),
        "externalEffectiveTargetTotals": scoped_totals("external"),
    }


def _entity_population_source_identity_joins(
    disasm: Path, addresses: dict[str, int]
) -> dict[str, Any]:
    """Join neighboring contracts by pinned source identity, never copied fixture payloads."""
    owner_specs = (
        ("code/common/scripting/entity/entityfunctions_1.asm", ("InitializeNewEntity",), None),
        (
            "code/common/scripting/map/mapfunctions.asm",
            ("InitializeMapEntities",),
            "sf2-map-entities-static-v1",
        ),
        (
            "code/common/scripting/entity/entityscriptengine_2.asm",
            ("LoadEntityMapsprites",),
            None,
        ),
        (
            "code/common/scripting/map/mapsetupsfunctions_1.asm",
            ("GetMapSetupEntityList",),
            "sf2-map-setup-static-v1",
        ),
        (
            "code/common/tech/interrupts/vdpcontrol.asm",
            ("DisableDisplayAndInterrupts", "EnableDisplayAndInterrupts"),
            None,
        ),
        (
            "code/common/scripting/map/mapscriptengine_1.asm",
            ("GetEntityAddressFromCharacter",),
            None,
        ),
    )
    callee_owners = []
    for source_path, symbols, related_contract_id in owner_specs:
        source = read_upstream_text(disasm / source_path)
        for symbol in symbols:
            has_source_label = re.search(
                rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE
            )
            if symbol not in addresses or has_source_label is None:
                raise ValueError(f"entity-population source owner symbol drift: {symbol}")
        callee_owners.append(
            {
                "sourcePath": source_path,
                "sourceSha256": hashlib.sha256(source.encode()).hexdigest().upper(),
                "symbols": list(symbols),
                "relatedContractId": related_contract_id,
            }
        )
    init_path = "data/scripting/entity/eas_actions.asm"
    init_source = read_upstream_text(disasm / init_path)
    if "eas_Init" not in addresses or re.search(
        r"^eas_Init:\s*", init_source, re.MULTILINE
    ) is None:
        raise ValueError("entity-population entity-action initializer identity drift")
    return {
        "calleeOwners": callee_owners,
        "entityActionInitializer": {
            "sourcePath": init_path,
            "sourceSha256": hashlib.sha256(init_source.encode()).hexdigest().upper(),
            "symbol": "eas_Init",
            "address": addresses["eas_Init"],
            "relatedContractId": "sf2-entity-action-scripts-static-v1",
        },
    }


def _entity_population_command_facts(
    disasm: Path,
    equates: dict[str, int],
    macros: dict[str, dict[str, Any]],
    dispatch_targets: list[str],
    handlers: list[dict[str, Any]],
    program_corpus: dict[str, Any],
    addresses: dict[str, int],
    rom: bytes,
) -> dict[str, Any]:
    """Build the static source/handler/caller contract for entity-population forms."""
    annotations_by_macro = _entity_population_macro_annotations(disasm)
    source_sites, program_totals = _entity_population_program_facts(
        program_corpus, annotations_by_macro, equates
    )
    source_counts: Counter[str] = Counter(
        command["macro"] for site in source_sites for command in site["commands"]
    )
    for macro in ENTITY_POPULATION_MACRO_NAMES:
        if sum(row["macroCounts"][macro] for row in program_totals) != source_counts[macro]:
            raise ValueError(f"entity-population program total drift: {macro}")
        contract = macros[macro]
        annotations = annotations_by_macro[macro]
        source_operand_bytes = sum(row["widthBytes"] for row in annotations)
        source_encoded_bytes = max(
            row["streamOffset"] + row["widthBytes"] for row in annotations
        )
        if (
            contract["kind"] != "command"
            or contract["opcode"] is None
            or contract["operandBytes"] != source_operand_bytes
            or contract["encodedBytes"] != source_encoded_bytes
            or contract["operandLayout"]
            != [
                {
                    "streamOffset": row["streamOffset"],
                    "widthBytes": row["widthBytes"],
                    "expression": f"\\{row['parameterOrdinal']}",
                    "parameterOrdinals": [row["parameterOrdinal"]],
                    "encoding": "direct",
                }
                for row in annotations
            ]
        ):
            raise ValueError(f"entity-population macro ABI drift: {macro}")

    handler_rows = []
    direct_call_rows: dict[str, list[dict[str, str]]] = {}
    for macro in ENTITY_POPULATION_MACRO_NAMES:
        handler_name = ENTITY_POPULATION_HANDLER_BY_MACRO[macro]
        contract = macros[macro]
        opcode = contract["opcode"]
        if opcode is None or dispatch_targets[opcode] != handler_name:
            raise ValueError(f"entity-population dispatcher target drift: {macro}")
        handler = _handler_by_name(handlers, handler_name)
        if (
            handler["opcodes"] != [opcode]
            or handler["encodedCommandBytes"] != contract["encodedBytes"]
        ):
            raise ValueError(f"entity-population handler ABI drift: {handler_name}")
        statements = _stable_handler_statements(disasm, handler)
        section_guard = _entity_population_section_guard(
            macro, statements, equates, addresses
        )
        script_cursor_bytes = sum(
            row["transferredByteCount"] for row in section_guard["scriptCursorReadUseSites"]
        )
        if script_cursor_bytes != contract["operandBytes"]:
            raise ValueError(f"entity-population script cursor width drift: {macro}")
        direct_calls = _force_state_direct_calls(statements)
        if direct_calls != _force_state_direct_calls(section_guard["directCallOrder"]):
            raise ValueError(f"entity-population direct-call order drift: {macro}")
        direct_call_rows[handler_name] = direct_calls
        handler_rows.append(
            {
                "macro": macro,
                "handler": handler_name,
                "address": handler["address"],
                "opcode": opcode,
                "sourceCommandCount": source_counts[macro],
                "operandAnnotations": annotations_by_macro[macro],
                "statementCount": len(statements),
                "guardedStatements": statements,
                "sectionGuard": section_guard,
                "directCalls": direct_calls,
            }
        )
    return {
        "macros": [
            {
                "name": macro,
                "opcode": macros[macro]["opcode"],
                "encodedBytes": macros[macro]["encodedBytes"],
                "operandBytes": macros[macro]["operandBytes"],
                "operandLayout": macros[macro]["operandLayout"],
                "parameterOrdinals": macros[macro]["parameterOrdinals"],
                "handler": ENTITY_POPULATION_HANDLER_BY_MACRO[macro],
                "sourceOperandAnnotations": annotations_by_macro[macro],
                "sourceCommandCount": source_counts[macro],
            }
            for macro in ENTITY_POPULATION_MACRO_NAMES
        ],
        "sourceSites": source_sites,
        **_entity_population_corpus_order_facts(source_sites, program_totals),
        "programTotals": program_totals,
        "handlers": handler_rows,
        "callerBreakdown": _entity_population_caller_breakdown(
            disasm, handlers, direct_call_rows, addresses, rom
        ),
        "sourceIdentityJoins": _entity_population_source_identity_joins(disasm, addresses),
        "runtimeQuestions": ENTITY_POPULATION_RUNTIME_QUESTIONS,
    }


def _map_lifecycle_macro_annotations(disasm: Path) -> dict[str, list[dict[str, Any]]]:
    """Parse the bounded macro operand comments with their emitted byte shape."""
    blocks = _macro_blocks(read_upstream_text(disasm / MACRO_PATH))
    annotations_by_macro: dict[str, list[dict[str, Any]]] = {}
    for macro in MAP_LIFECYCLE_MACRO_NAMES:
        body = blocks.get(macro)
        if body is None:
            raise ValueError(f"map-lifecycle macro is missing: {macro}")
        parameter_rows = [row for row in _emission_rows(body) if row["parameterOrdinals"]]
        annotations: list[dict[str, Any]] = []
        for raw_line in body.splitlines():
            match = re.fullmatch(
                r"\s*dc\.[bwl]\s+\\(?P<ordinal>\d+)"
                r"(?:\s*;\s*(?P<comment>.*))?\s*",
                raw_line,
            )
            if match is None:
                continue
            if len(annotations) >= len(parameter_rows):
                raise ValueError(f"map-lifecycle operand emission drift: {macro}")
            row = parameter_rows[len(annotations)]
            ordinal = int(match.group("ordinal"))
            if row["parameterOrdinals"] != [ordinal]:
                raise ValueError(f"map-lifecycle operand ordinal drift: {macro}")
            comment = match.group("comment")
            if comment is None:
                raise ValueError(f"map-lifecycle operand comment is missing: {macro}")
            annotations.append(
                {
                    "parameterOrdinal": ordinal,
                    "sourceComment": comment,
                    "streamOffset": row["streamOffset"],
                    "widthBytes": row["widthBytes"],
                }
            )
        if len(annotations) != len(parameter_rows):
            raise ValueError(f"map-lifecycle operand comment coverage drift: {macro}")
        if [row["parameterOrdinal"] for row in annotations] != list(
            range(1, len(annotations) + 1)
        ):
            raise ValueError(f"map-lifecycle operand ordinal sequence drift: {macro}")
        annotations_by_macro[macro] = annotations
    return annotations_by_macro


def _map_lifecycle_resolve_operand(value: str, equates: dict[str, int]) -> dict[str, Any]:
    """Resolve one source operand against the single parsed map-script equate map."""
    token = value.strip()
    if token in equates:
        return {"rawValue": value, "resolvedValue": equates[token], "resolution": "equate"}
    try:
        return {"rawValue": value, "resolvedValue": _literal(token), "resolution": "literal"}
    except ValueError:
        return {"rawValue": value, "resolvedValue": None, "resolution": "symbol"}


def _map_lifecycle_program_facts(
    program_corpus: dict[str, Any],
    annotations_by_macro: dict[str, list[dict[str, Any]]],
    equates: dict[str, int],
    canonical_map_ids: set[int],
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    """Retain every bounded source use and all zero-inclusive program totals."""
    source_sites, program_totals = _force_state_program_facts(
        program_corpus, macro_names=MAP_LIFECYCLE_MACRO_NAMES
    )
    annotated_sites: list[dict[str, Any]] = []
    for site in source_sites:
        commands = []
        for command in site["commands"]:
            annotations = annotations_by_macro[command["macro"]]
            if len(command["arguments"]) != len(annotations):
                raise ValueError(
                    "map-lifecycle source operand count drift: "
                    f"{site['programId']}:{command['commandIndex']}"
                )
            operand_values = []
            for annotation, argument in zip(annotations, command["arguments"], strict=True):
                resolved = _map_lifecycle_resolve_operand(argument, equates)
                map_domain: str | None = None
                if annotation["sourceComment"] == "map":
                    value = resolved["resolvedValue"]
                    if value in canonical_map_ids:
                        map_domain = "canonical-map"
                    elif argument == "MAP_CURRENT" and value == equates["MAP_CURRENT"]:
                        map_domain = "source-map-current"
                    else:
                        raise ValueError("map-lifecycle map operand domain drift")
                operand_values.append(
                    {
                        "parameterOrdinal": annotation["parameterOrdinal"],
                        "sourceComment": annotation["sourceComment"],
                        "streamOffset": annotation["streamOffset"],
                        "widthBytes": annotation["widthBytes"],
                        **resolved,
                        "mapDomain": map_domain,
                    }
                )
            commands.append(
                {
                    **command,
                    "sourceOrderKey": ":".join(
                        (site["programId"], str(command["commandIndex"]), command["macro"])
                    ),
                    "operandValues": operand_values,
                }
            )
        annotated_sites.append({"programId": site["programId"], "commands": commands})
    return annotated_sites, program_totals


def _map_lifecycle_corpus_order_facts(
    source_sites: list[dict[str, Any]], program_totals: list[dict[str, Any]]
) -> dict[str, Any]:
    """Bind the full ordered corpora compactly without expanding them in schemas."""
    source_order_keys = [
        command["sourceOrderKey"] for site in source_sites for command in site["commands"]
    ]
    if len(source_order_keys) != len(set(source_order_keys)):
        raise ValueError("map-lifecycle source order keys are not unique")
    program_order_keys = [row["programId"] for row in program_totals]
    if len(program_order_keys) != len(set(program_order_keys)):
        raise ValueError("map-lifecycle program-total order keys are not unique")
    return {
        "sourceSiteOrderKeys": source_order_keys,
        "sourceSitesSha256": hashlib.sha256(
            _canonical_bytes({"sourceSites": source_sites})
        ).hexdigest().upper(),
        "programTotalOrderKeys": program_order_keys,
        "programTotalsSha256": hashlib.sha256(
            _canonical_bytes({"programTotals": program_totals})
        ).hexdigest().upper(),
    }


def _map_lifecycle_read_use_site(instruction: str) -> dict[str, Any]:
    """Parse one bounded A6 cursor/probe read and keep transfer separate from advance."""
    match = re.fullmatch(
        r"move\.(?P<size>[bwl]) \((?P<source>a[06])\)(?P<increment>\+)?,(?P<target>d[0-7])",
        instruction,
    )
    if match is None:
        raise ValueError("map-lifecycle read use shape drift")
    transferred_bytes = {"b": 1, "w": 2, "l": 4}[match.group("size")]
    return {
        "sourceRegister": match.group("source"),
        "destinationRegister": match.group("target"),
        "transferredByteCount": transferred_bytes,
        "cursorAdvanceByteCount": transferred_bytes if match.group("increment") else 0,
        "instruction": instruction,
    }


def _map_lifecycle_vint_record(
    statements: list[str], index: int, phase: str, equates: dict[str, int]
) -> dict[str, Any]:
    """Tie a VInt phase to its exact trap and adjacent data records."""
    symbol = "VINTS_DEACTIVATE" if phase == "deactivate" else "VINTS_ACTIVATE"
    expected = ["trap #VINT_FUNCTIONS", f"dc.w {symbol}", "dc.l 0"]
    if statements[index : index + len(expected)] != expected:
        raise ValueError(f"map-lifecycle VInt {phase} use-site order drift")
    if symbol not in equates:
        raise ValueError(f"map-lifecycle VInt source constant is missing: {symbol}")
    return {
        "phase": phase,
        "operationSymbol": symbol,
        "operationValue": equates[symbol],
        "trapInstruction": statements[index],
        "operationInstruction": statements[index + 1],
        "argumentInstruction": statements[index + 2],
    }


def _map_lifecycle_constant_use(
    symbol: str, instruction: str, pattern: str, equates: dict[str, int]
) -> dict[str, Any]:
    """Record a parsed source equate only when its named instruction use is exact."""
    if symbol not in equates or re.fullmatch(pattern, instruction) is None:
        raise ValueError(f"map-lifecycle source constant use drift: {symbol}")
    return {"symbol": symbol, "value": equates[symbol], "instruction": instruction}


def _map_lifecycle_operand_pack_use_sites(
    annotations: list[dict[str, Any]],
    statements: list[str],
    *,
    shift_index: int,
    mask_index: int,
    merge_index: int,
    multiplier_index: int,
    equates: dict[str, int],
) -> dict[str, Any]:
    """Bind camera-comment operands to the exact D0/D2 transform use sites."""
    camera_annotations = annotations[-2:]
    if [row["sourceComment"] for row in camera_annotations] != ["camera X", "camera Y"]:
        raise ValueError("map-lifecycle camera operand annotation drift")
    multiplier_match = re.fullmatch(r"mulu\.w #(?P<value>\d+),d0", statements[multiplier_index])
    if multiplier_match is None:
        raise ValueError("map-lifecycle operand multiplier use-site drift")
    if statements[merge_index] != "or.w d2,d0":
        raise ValueError("map-lifecycle operand merge use-site drift")
    return {
        "parameterOrdinals": [row["parameterOrdinal"] for row in camera_annotations],
        "sourceComments": [row["sourceComment"] for row in camera_annotations],
        "shiftUseSite": _map_lifecycle_constant_use(
            "BYTE_SHIFT_COUNT",
            statements[shift_index],
            r"lsl\.w #BYTE_SHIFT_COUNT,d0",
            equates,
        ),
        "maskUseSite": _map_lifecycle_constant_use(
            "BYTE_MASK",
            statements[mask_index],
            r"andi\.w #BYTE_MASK,d2",
            equates,
        ),
        "mergeInstruction": statements[merge_index],
        "multiplierUseSite": {
            "value": _literal(multiplier_match.group("value")),
            "instruction": statements[multiplier_index],
        },
    }


def _map_lifecycle_section_guard(
    macro: str,
    statements: list[str],
    annotations: list[dict[str, Any]],
    equates: dict[str, int],
) -> dict[str, Any]:
    """Guard complete named sections, including cursor, branch, VInt, call, and mutation order."""
    if macro == "resetMap":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.l a6,-\(sp\)",
                r"jsr \(ResetCurrentMap\)\.l",
                r"movea\.l \(sp\)\+,a6",
                r"rts",
            ],
            owner="csc36_resetMap",
        )
        cursor_indexes: tuple[int, ...] = ()
        map_probe: dict[str, Any] | None = None
        vint_records: list[dict[str, Any]] = []
        branch_records: list[dict[str, Any]] = []
        mutation_records: list[dict[str, Any]] = []
        source_constant_uses: list[dict[str, Any]] = []
        operand_pack_use_sites: dict[str, Any] | None = None
        selector_use_site: dict[str, Any] | None = None
        direct_call_order = [ordered[1]]
    elif macro == "loadMapFadeIn":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.b #OUT_TO_BLACK,\(\(FADING_SETTING-\$1000000\)\)\.w",
                r"clr\.w \(FADING_TIMER_WORD\)\.l",
                r"clr\.b \(\(FADING_POINTER-\$1000000\)\)\.w",
                (
                    r"move\.b \(\(FADING_COUNTER_MAX-\$1000000\)\)\.w,"
                    r"\(\(FADING_COUNTER-\$1000000\)\)\.w"
                ),
                r"move\.b #%1111,\(\(FADING_PALETTE_BITFIELD-\$1000000\)\)\.w",
            ],
            owner="csc37_loadMapAndFadeIn",
        )
        if "OUT_TO_BLACK" not in equates:
            raise ValueError("map-lifecycle fade source constant is missing")
        cursor_indexes = ()
        map_probe = None
        vint_records = []
        branch_records = []
        mutation_records = [
            {
                "sourceSymbols": ["FADING_SETTING"],
                "sourceValueSymbol": "OUT_TO_BLACK",
                "sourceValue": equates["OUT_TO_BLACK"],
                "literalValue": None,
                "instruction": ordered[0],
            },
            {
                "sourceSymbols": ["FADING_TIMER_WORD"],
                "sourceValueSymbol": None,
                "sourceValue": None,
                "literalValue": None,
                "instruction": ordered[1],
            },
            {
                "sourceSymbols": ["FADING_POINTER"],
                "sourceValueSymbol": None,
                "sourceValue": None,
                "literalValue": None,
                "instruction": ordered[2],
            },
            {
                "sourceSymbols": ["FADING_COUNTER_MAX", "FADING_COUNTER"],
                "sourceValueSymbol": None,
                "sourceValue": None,
                "literalValue": None,
                "instruction": ordered[3],
            },
            {
                "sourceSymbols": ["FADING_PALETTE_BITFIELD"],
                "sourceValueSymbol": None,
                "sourceValue": None,
                "literalValue": 15,
                "instruction": ordered[4],
            },
        ]
        source_constant_uses = [
            {
                "symbol": "OUT_TO_BLACK",
                "value": equates["OUT_TO_BLACK"],
                "instruction": ordered[0],
            }
        ]
        operand_pack_use_sites = None
        selector_use_site = None
        direct_call_order = []
    elif macro == "mapLoad":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.b #-1,\(\(VIEW_TARGET_ENTITY-\$1000000\)\)\.w",
                r"nop",
                r"move\.w \(a6\),d1",
                r"jsr \(LoadMapTilesets\)\.w",
                r"jsr \(WaitForVInt\)\.w",
                r"tst\.b \(\(FADING_SETTING-\$1000000\)\)\.w",
                r"bne\.s [A-Za-z_][A-Za-z0-9_]*",
                r"trap #VINT_FUNCTIONS",
                r"dc\.w VINTS_DEACTIVATE",
                r"dc\.l 0",
                r"clr\.l d0",
                r"move\.w \(a6\)\+,d1",
                r"move\.w \(a6\)\+,d0",
                r"lsl\.w #BYTE_SHIFT_COUNT,d0",
                r"move\.w \(a6\)\+,d2",
                r"andi\.w #BYTE_MASK,d2",
                r"or\.w d2,d0",
                r"mulu\.w #\d+,d0",
                r"move\.l a6,-\(sp\)",
                r"jsr \(LoadMap\)\.w",
                r"movea\.l \(sp\)\+,a6",
                r"jsr \(EnableDisplayAndInterrupts\)\.w",
                r"trap #VINT_FUNCTIONS",
                r"dc\.w VINTS_ACTIVATE",
                r"dc\.l 0",
                r"jsr \(WaitForVInt\)\.w",
                r"rts",
            ],
            owner="csc48_loadMap",
        )
        cursor_indexes = (11, 12, 14)
        map_probe = _map_lifecycle_read_use_site(ordered[2])
        vint_records = [
            _map_lifecycle_vint_record(ordered, 7, "deactivate", equates),
            _map_lifecycle_vint_record(ordered, 22, "activate", equates),
        ]
        branch_records = [
            {
                "testInstruction": ordered[5],
                "branchInstruction": ordered[6],
                "fallthroughInstruction": ordered[7],
            }
        ]
        mutation_records = [
            {
                "sourceSymbols": ["VIEW_TARGET_ENTITY"],
                "sourceValueSymbol": None,
                "sourceValue": None,
                "literalValue": -1,
                "instruction": ordered[0],
            }
        ]
        source_constant_uses = []
        operand_pack_use_sites = _map_lifecycle_operand_pack_use_sites(
            annotations,
            ordered,
            shift_index=13,
            mask_index=15,
            merge_index=16,
            multiplier_index=17,
            equates=equates,
        )
        selector_use_site = None
        direct_call_order = [ordered[index] for index in (3, 4, 19, 21, 25)]
    elif macro == "reloadMap":
        ordered = _force_state_ordered_statements(
            statements,
            [
                r"move\.b #-1,\(\(VIEW_TARGET_ENTITY-\$1000000\)\)\.w",
                r"nop",
                r"trap #VINT_FUNCTIONS",
                r"dc\.w VINTS_DEACTIVATE",
                r"dc\.l 0",
                r"clr\.l d0",
                r"moveq #-1,d1",
                r"move\.w \(a6\)\+,d0",
                r"lsl\.w #BYTE_SHIFT_COUNT,d0",
                r"move\.w \(a6\)\+,d2",
                r"andi\.w #BYTE_MASK,d2",
                r"or\.w d2,d0",
                r"mulu\.w #\d+,d0",
                r"move\.l a6,-\(sp\)",
                r"jsr \(LoadMap\)\.w",
                r"movea\.l \(sp\)\+,a6",
                r"jsr \(EnableDisplayAndInterrupts\)\.w",
                r"trap #VINT_FUNCTIONS",
                r"dc\.w VINTS_ACTIVATE",
                r"dc\.l 0",
                r"jsr \(WaitForVInt\)\.w",
                r"rts",
            ],
            owner="csc46_reloadMap",
        )
        cursor_indexes = (7, 9)
        map_probe = None
        vint_records = [
            _map_lifecycle_vint_record(ordered, 2, "deactivate", equates),
            _map_lifecycle_vint_record(ordered, 17, "activate", equates),
        ]
        branch_records = []
        mutation_records = [
            {
                "sourceSymbols": ["VIEW_TARGET_ENTITY"],
                "sourceValueSymbol": None,
                "sourceValue": None,
                "literalValue": -1,
                "instruction": ordered[0],
            }
        ]
        source_constant_uses = []
        operand_pack_use_sites = _map_lifecycle_operand_pack_use_sites(
            annotations,
            ordered,
            shift_index=8,
            mask_index=10,
            merge_index=11,
            multiplier_index=12,
            equates=equates,
        )
        selector_use_site = {"literalValue": -1, "instruction": ordered[6]}
        direct_call_order = [ordered[index] for index in (14, 16, 20)]
    else:
        raise ValueError(f"map-lifecycle guard has no macro profile: {macro}")
    if len(statements) != len(ordered):
        raise ValueError(f"map-lifecycle handler statement coverage drift: {macro}")
    return {
        "orderedInstructions": ordered,
        "scriptCursorReadUseSites": [
            _map_lifecycle_read_use_site(ordered[index]) for index in cursor_indexes
        ],
        "mapProbeUseSite": map_probe,
        "vintControlRecords": vint_records,
        "branchRecords": branch_records,
        "sourceStateMutationRecords": mutation_records,
        "sourceConstantUses": source_constant_uses,
        "operandPackUseSites": operand_pack_use_sites,
        "sourceD1SelectorUseSite": selector_use_site,
        "directCallOrder": direct_call_order,
        "returnInstruction": ordered[-1] if ordered[-1] == "rts" else None,
    }


def _map_lifecycle_named_section_source(disasm: Path, handler: dict[str, Any]) -> str:
    """Read one bounded named section with labels retained for local target resolution."""
    source = read_upstream_text(disasm / handler["sourcePath"])
    match = re.search(
        rf"^{re.escape(handler['name'])}:\s*\n(?P<body>.*?)"
        rf"^\s*; End of function {re.escape(handler['name'])}\s*$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if match is None:
        raise ValueError(f"map-lifecycle named section is missing: {handler['name']}")
    return match.group("body")


def _map_lifecycle_branch_target_record(
    section_source: str, branch_instruction: str, ordered_instructions: list[str]
) -> dict[str, Any]:
    """Resolve csc48's guarded branch operand to its labeled first VInt wait site."""
    branch = re.fullmatch(r"bne\.s (?P<label>[A-Za-z_][A-Za-z0-9_]*)", branch_instruction)
    if branch is None:
        raise ValueError("map-lifecycle branch target instruction drift")
    label = branch.group("label")
    if label != "loc_465C4":
        raise ValueError("map-lifecycle branch target label drift")
    label_match = re.search(rf"^{re.escape(label)}:\s*$", section_source, re.MULTILINE)
    if label_match is None:
        raise ValueError("map-lifecycle branch target label is missing")
    target_statements = _statements(section_source[label_match.end() :])
    if not target_statements:
        raise ValueError("map-lifecycle branch target has no instruction")
    target_instruction = target_statements[0]
    target_statement_index = len(_statements(section_source[: label_match.start()]))
    if (
        target_instruction != "jsr (WaitForVInt).w"
        or target_statement_index >= len(ordered_instructions)
        or ordered_instructions[target_statement_index] != target_instruction
    ):
        raise ValueError("map-lifecycle branch target instruction drift")
    wait_indexes = [
        index
        for index, instruction in enumerate(ordered_instructions)
        if instruction == "jsr (WaitForVInt).w"
    ]
    if not wait_indexes or target_statement_index != wait_indexes[0]:
        raise ValueError("map-lifecycle branch target first-wait relationship drift")
    return {
        "targetLabel": label,
        "targetInstruction": target_instruction,
        "targetStatementIndex": target_statement_index,
    }


def _map_lifecycle_fallthrough_guard(disasm: Path) -> None:
    """Prove that the fade section has no independent return before csc48 begins."""
    source = read_upstream_text(disasm / "code/common/scripting/map/mapscriptengine_1.asm")
    fade = re.search(r"^csc37_loadMapAndFadeIn:\s*$", source, re.MULTILINE)
    load = re.search(r"^csc48_loadMap:\s*$", source, re.MULTILINE)
    if fade is None or load is None or fade.start() >= load.start():
        raise ValueError("map-lifecycle fade/load source order drift")
    between = source[fade.end() : load.start()]
    if re.search(r"\brts\b", between) or re.search(
        r"^[A-Za-z_][A-Za-z0-9_]*:\s*$", between, re.MULTILINE
    ):
        raise ValueError("map-lifecycle fade/load fallthrough drift")


def _map_lifecycle_caller_breakdown(
    disasm: Path,
    direct_call_rows: dict[str, list[dict[str, str]]],
    addresses: dict[str, int],
    rom: bytes,
) -> dict[str, Any]:
    """Keep parsed direct targets and resolved effective targets as separate identities."""
    instruction_targets = sorted(
        {call["instructionTarget"] for rows in direct_call_rows.values() for call in rows}
    )
    aliases = _force_state_aliases(disasm, set(instruction_targets), addresses, rom)
    bounded_handlers = set(MAP_LIFECYCLE_HANDLER_NAMES)
    target_resolutions = []
    for target in instruction_targets:
        effective_target = aliases.get(target, {}).get("effectiveTarget", target)
        target_resolutions.append(
            {
                "instructionTarget": target,
                "effectiveTarget": effective_target,
                "aliasSourcePath": aliases.get(target, {}).get("sourcePath"),
                "effectiveTargetScope": (
                    "internal" if effective_target in bounded_handlers else "external"
                ),
            }
        )
    effective_targets = sorted({row["effectiveTarget"] for row in target_resolutions})
    resolution_by_instruction = {
        row["instructionTarget"]: row["effectiveTarget"] for row in target_resolutions
    }
    caller_rows = []
    for handler_name in MAP_LIFECYCLE_HANDLER_NAMES:
        instruction_counts = {target: 0 for target in instruction_targets}
        effective_counts = {target: 0 for target in effective_targets}
        for call in direct_call_rows[handler_name]:
            instruction_counts[call["instructionTarget"]] += 1
            effective_counts[resolution_by_instruction[call["instructionTarget"]]] += 1
        caller_rows.append(
            {
                "handler": handler_name,
                "instructionTargetSiteCounts": instruction_counts,
                "effectiveTargetSiteCounts": effective_counts,
            }
        )
    instruction_totals = {
        target: sum(row["instructionTargetSiteCounts"][target] for row in caller_rows)
        for target in instruction_targets
    }
    effective_totals = {
        target: sum(row["effectiveTargetSiteCounts"][target] for row in caller_rows)
        for target in effective_targets
    }
    scopes = {row["effectiveTarget"]: row["effectiveTargetScope"] for row in target_resolutions}

    def scoped_totals(scope: str) -> dict[str, int]:
        return {
            target: effective_totals[target] if scopes[target] == scope else 0
            for target in effective_targets
        }

    return {
        "callerHandlers": caller_rows,
        "targetResolutions": target_resolutions,
        "instructionTargetTotals": instruction_totals,
        "effectiveTargetTotals": effective_totals,
        "internalEffectiveTargetTotals": scoped_totals("internal"),
        "externalEffectiveTargetTotals": scoped_totals("external"),
    }


def _map_lifecycle_source_identity_joins(
    disasm: Path, addresses: dict[str, int]
) -> dict[str, Any]:
    """Record the bounded handler source and independent owner identities."""
    handler_path = "code/common/scripting/map/mapscriptengine_1.asm"
    handler_source = read_upstream_text(disasm / handler_path)
    if any(
        re.search(rf"^{re.escape(symbol)}:\s*$", handler_source, re.MULTILINE) is None
        for symbol in MAP_LIFECYCLE_HANDLER_NAMES
    ):
        raise ValueError("map-lifecycle handler source identity drift")
    owner_specs = (
        ("code/gameflow/exploration/exploration.asm", ("ResetCurrentMap",), None),
        ("code/common/maps/mapload.asm", ("LoadMapTilesets", "LoadMap"), None),
        ("code/common/tech/interrupts/vdpcontrol.asm", ("EnableDisplayAndInterrupts",), None),
        ("code/common/tech/interrupts/vintengine_1.asm", ("WaitForVInt",), None),
    )
    owners = []
    for source_path, symbols, related_contract_id in owner_specs:
        source = read_upstream_text(disasm / source_path)
        for symbol in symbols:
            if symbol not in addresses or re.search(
                rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE
            ) is None:
                raise ValueError(f"map-lifecycle source owner symbol drift: {symbol}")
        owners.append(
            {
                "sourcePath": source_path,
                "sourceSha256": hashlib.sha256(source.encode()).hexdigest().upper(),
                "symbols": list(symbols),
                "relatedContractId": related_contract_id,
            }
        )
    return {
        "handlerSource": {
            "sourcePath": handler_path,
            "sourceSha256": hashlib.sha256(handler_source.encode()).hexdigest().upper(),
            "symbols": list(MAP_LIFECYCLE_HANDLER_NAMES),
        },
        "calleeOwners": owners,
    }


def _map_lifecycle_command_facts(
    disasm: Path,
    equates: dict[str, int],
    macros: dict[str, dict[str, Any]],
    dispatch_targets: list[str],
    handlers: list[dict[str, Any]],
    program_corpus: dict[str, Any],
    addresses: dict[str, int],
    rom: bytes,
    rom_path: Path,
    upstream_path: Path,
) -> dict[str, Any]:
    """Build the static contract for reset/load/reload forms without runtime interpretation."""
    required_equates = (
        "MAP_CURRENT",
        "BYTE_SHIFT_COUNT",
        "BYTE_MASK",
        "VINTS_DEACTIVATE",
        "VINTS_ACTIVATE",
    )
    missing = [name for name in required_equates if name not in equates]
    if missing:
        raise ValueError(f"map-lifecycle source constants are missing: {missing}")
    map_content = build_map_content_contract(rom_path, upstream_path)
    canonical_map_ids = {row["map"] for row in map_content["mapEntries"]}
    if len(canonical_map_ids) != map_content["summary"]["mapCount"]:
        raise ValueError("map-lifecycle canonical map identity drift")
    annotations_by_macro = _map_lifecycle_macro_annotations(disasm)
    source_sites, program_totals = _map_lifecycle_program_facts(
        program_corpus, annotations_by_macro, equates, canonical_map_ids
    )
    source_counts: Counter[str] = Counter(
        command["macro"] for site in source_sites for command in site["commands"]
    )
    for macro in MAP_LIFECYCLE_MACRO_NAMES:
        if sum(row["macroCounts"][macro] for row in program_totals) != source_counts[macro]:
            raise ValueError(f"map-lifecycle program total drift: {macro}")
        contract = macros[macro]
        annotations = annotations_by_macro[macro]
        source_operand_bytes = sum(row["widthBytes"] for row in annotations)
        source_encoded_bytes = (
            max((row["streamOffset"] + row["widthBytes"] for row in annotations), default=2)
        )
        if (
            contract["kind"] != "command"
            or contract["opcode"] is None
            or contract["operandBytes"] != source_operand_bytes
            or contract["encodedBytes"] != source_encoded_bytes
            or contract["operandLayout"]
            != [
                {
                    "streamOffset": row["streamOffset"],
                    "widthBytes": row["widthBytes"],
                    "expression": f"\\{row['parameterOrdinal']}",
                    "parameterOrdinals": [row["parameterOrdinal"]],
                    "encoding": "direct",
                }
                for row in annotations
            ]
        ):
            raise ValueError(f"map-lifecycle macro ABI drift: {macro}")

    handler_rows = []
    direct_call_rows: dict[str, list[dict[str, str]]] = {}
    guards_by_macro: dict[str, dict[str, Any]] = {}
    handlers_by_macro: dict[str, dict[str, Any]] = {}
    for macro in MAP_LIFECYCLE_MACRO_NAMES:
        handler_name = MAP_LIFECYCLE_HANDLER_BY_MACRO[macro]
        contract = macros[macro]
        opcode = contract["opcode"]
        if opcode is None or dispatch_targets[opcode] != handler_name:
            raise ValueError(f"map-lifecycle dispatcher target drift: {macro}")
        handler = _handler_by_name(handlers, handler_name)
        if (
            handler["opcodes"] != [opcode]
            or handler["encodedCommandBytes"] != contract["encodedBytes"]
        ):
            raise ValueError(f"map-lifecycle handler ABI drift: {handler_name}")
        statements = _stable_handler_statements(disasm, handler)
        section_guard = _map_lifecycle_section_guard(
            macro, statements, annotations_by_macro[macro], equates
        )
        if macro == "mapLoad":
            section_guard["branchRecords"][0]["branchTarget"] = (
                _map_lifecycle_branch_target_record(
                    _map_lifecycle_named_section_source(disasm, handler),
                    section_guard["branchRecords"][0]["branchInstruction"],
                    section_guard["orderedInstructions"],
                )
            )
        direct_calls = _force_state_direct_calls(statements)
        if direct_calls != _force_state_direct_calls(section_guard["directCallOrder"]):
            raise ValueError(f"map-lifecycle direct-call order drift: {macro}")
        direct_call_rows[handler_name] = direct_calls
        guards_by_macro[macro] = section_guard
        handlers_by_macro[macro] = handler
        handler_rows.append(
            {
                "macro": macro,
                "handler": handler_name,
                "address": handler["address"],
                "opcode": opcode,
                "sourceCommandCount": source_counts[macro],
                "operandAnnotations": annotations_by_macro[macro],
                "statementCount": len(statements),
                "guardedStatements": statements,
                "sectionGuard": section_guard,
                "directCalls": direct_calls,
                "continuation": None,
            }
        )
    _map_lifecycle_fallthrough_guard(disasm)
    map_load_guard = guards_by_macro["mapLoad"]
    load_fade_row = next(row for row in handler_rows if row["macro"] == "loadMapFadeIn")
    load_fade_row["continuation"] = {
        "handler": handlers_by_macro["mapLoad"]["name"],
        "address": handlers_by_macro["mapLoad"]["address"],
        "sectionGuard": map_load_guard,
        "directCalls": direct_call_rows[handlers_by_macro["mapLoad"]["name"]],
    }
    for macro in ("loadMapFadeIn", "mapLoad", "reloadMap"):
        guard = map_load_guard if macro == "loadMapFadeIn" else guards_by_macro[macro]
        cursor_bytes = sum(
            row["cursorAdvanceByteCount"] for row in guard["scriptCursorReadUseSites"]
        )
        if cursor_bytes != macros[macro]["operandBytes"]:
            raise ValueError(f"map-lifecycle script cursor width drift: {macro}")
    if (
        map_load_guard["operandPackUseSites"] is None
        or guards_by_macro["reloadMap"]["operandPackUseSites"] is None
        or map_load_guard["operandPackUseSites"]["multiplierUseSite"]
        != guards_by_macro["reloadMap"]["operandPackUseSites"]["multiplierUseSite"]
    ):
        raise ValueError("map-lifecycle shared operand-pack use-site drift")
    return {
        "macros": [
            {
                "name": macro,
                "opcode": macros[macro]["opcode"],
                "encodedBytes": macros[macro]["encodedBytes"],
                "operandBytes": macros[macro]["operandBytes"],
                "operandLayout": macros[macro]["operandLayout"],
                "parameterOrdinals": macros[macro]["parameterOrdinals"],
                "handler": MAP_LIFECYCLE_HANDLER_BY_MACRO[macro],
                "sourceOperandAnnotations": annotations_by_macro[macro],
                "sourceCommandCount": source_counts[macro],
            }
            for macro in MAP_LIFECYCLE_MACRO_NAMES
        ],
        "sourceSites": source_sites,
        **_map_lifecycle_corpus_order_facts(source_sites, program_totals),
        "programTotals": program_totals,
        "canonicalMapDomain": {
            "contractId": map_content["id"],
            "mapCount": map_content["summary"]["mapCount"],
            "mapIds": sorted(canonical_map_ids),
            "sourceMapCurrentValue": equates["MAP_CURRENT"],
        },
        "handlers": handler_rows,
        "callerBreakdown": _map_lifecycle_caller_breakdown(
            disasm, direct_call_rows, addresses, rom
        ),
        "sourceIdentityJoins": _map_lifecycle_source_identity_joins(disasm, addresses),
        "runtimeQuestions": MAP_LIFECYCLE_RUNTIME_QUESTIONS,
    }


def build_map_script_engine_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map-script H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    macros = _map_macro_contracts(disasm)
    source_counts, opcode_counts, source_paths = _source_usage(disasm, macros)
    program_corpus = _program_corpus(disasm, source_paths, macros, addresses)
    source_equates = _source_equates(disasm)
    if program_corpus["summary"]["commandCount"] != sum(source_counts.values()):
        raise ValueError("map-script program ownership does not cover every source command")
    dispatch_source = read_upstream_text(disasm / DISPATCH_SOURCE)
    targets = _dispatch_targets(dispatch_source)
    handlers = _handler_rows(disasm, addresses, targets, source_counts, macros)
    dialogue_command_facts = _dialogue_command_facts(
        disasm,
        source_equates,
        macros,
        targets,
        handlers,
        program_corpus,
        addresses,
        rom_path,
        upstream_path,
    )
    transition_command_facts = _transition_command_facts(
        disasm,
        source_equates,
        macros,
        targets,
        handlers,
        program_corpus,
        rom_path,
        upstream_path,
    )
    force_state_command_facts = _force_state_command_facts(
        disasm,
        source_equates,
        macros,
        targets,
        handlers,
        program_corpus,
        addresses,
        rom,
        upstream_path,
    )
    force_state_command_facts["activePartyCommandFacts"] = _active_party_command_facts(
        disasm,
        source_equates,
        macros,
        targets,
        handlers,
        program_corpus,
        addresses,
        rom,
        upstream_path,
    )
    story_state_command_facts = _story_state_command_facts(
        disasm,
        source_equates,
        macros,
        targets,
        handlers,
        program_corpus,
        addresses,
        rom,
        upstream_path,
    )
    map_block_mutation_command_facts = _map_block_mutation_command_facts(
        disasm,
        source_equates,
        macros,
        targets,
        handlers,
        program_corpus,
        addresses,
        rom,
    )
    entity_population_command_facts = _entity_population_command_facts(
        disasm,
        source_equates,
        macros,
        targets,
        handlers,
        program_corpus,
        addresses,
        rom,
    )
    map_lifecycle_command_facts = _map_lifecycle_command_facts(
        disasm,
        source_equates,
        macros,
        targets,
        handlers,
        program_corpus,
        addresses,
        rom,
        rom_path,
        upstream_path,
    )
    table_address = addresses["rjt_cutsceneScriptCommands"]
    table_bytes = rom[table_address : table_address + len(targets) * 2]
    expected_words = b"".join(
        ((addresses[target] - table_address) & 0xFFFF).to_bytes(2, "big")
        for target in targets
    )
    if table_bytes != expected_words:
        raise ValueError("map-script dispatcher source/ROM parity drift")

    primary = {
        name: row
        for name, row in macros.items()
        if row["kind"] == "command" and row["aliasOf"] is None
    }
    aliases = {name: row for name, row in macros.items() if row["aliasOf"] is not None}
    filler_indices = [index for index, target in enumerate(targets) if target == "csc_doNothing"]
    if {row["opcode"] for row in primary.values()} != set(range(90)) - set(
        filler_indices
    ):
        raise ValueError("map-script primary macros do not cover every non-filler opcode")
    family_counts = Counter(
        family for handler in handlers for family in handler["families"]
    )
    command_width_counts = Counter(row["encodedBytes"] for row in primary.values())
    handler_flow_counts = Counter(row["cursorFlow"] for row in handlers)
    summary = {
        "dispatcherSlotCount": len(targets),
        "uniqueHandlerCount": len(handlers),
        "fillerSlotCount": len(filler_indices),
        "primaryCommandMacroCount": len(primary),
        "aliasMacroCount": len(aliases),
        "specialMacroCount": sum(row["kind"] != "command" for row in macros.values()),
        "trackedMacroCount": len(macros),
        "usedMacroCount": sum(source_counts[name] > 0 for name in macros),
        "unusedMacroCount": sum(source_counts[name] == 0 for name in macros),
        "sourceCommandCount": sum(source_counts.values()),
        "sourceFileCount": len(source_paths),
        "handlerStatementCount": sum(row["statementCount"] for row in handlers),
        "handlerEntityFieldCount": len(
            {
                access["name"]
                for row in handlers
                for access in row["entityFieldAccesses"]
            }
        ),
        "handlerGlobalStateCount": len(
            {
                access["name"]
                for row in handlers
                for access in row["globalStateAccesses"]
            }
        ),
        "handlerDirectCallTargetCount": len(
            {call for row in handlers for call in row["directCalls"]}
        ),
        "primaryLogicalParameterCount": sum(
            len(row["parameterOrdinals"]) for row in primary.values()
        ),
        "primaryOperandFieldCount": sum(
            len(row["operandLayout"]) for row in primary.values()
        ),
        "primaryOperandByteCount": sum(row["operandBytes"] for row in primary.values()),
        "sequentialHandlerCount": handler_flow_counts["sequential"],
        "programCount": program_corpus["summary"]["programCount"],
        "programLabelCount": program_corpus["summary"]["programLabelCount"],
        "programTransferCount": program_corpus["summary"]["transferCount"],
        "encodedCommandByteCount": program_corpus["summary"][
            "encodedCommandByteCount"
        ],
        "referencedProgramCount": program_corpus["referenceSummary"][
            "referencedProgramCount"
        ],
        "unreferencedProgramCount": program_corpus["referenceSummary"][
            "unreferencedProgramCount"
        ],
        "statefulProgramCount": program_corpus["storyState"]["summary"][
            "statefulProgramCount"
        ],
        "storyReadFlagCount": program_corpus["storyState"]["summary"][
            "uniqueReadFlagCount"
        ],
        "storyWriteFlagCount": program_corpus["storyState"]["summary"][
            "uniqueWriteFlagCount"
        ],
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "rom": {"sha256": inspect_rom(rom_path)["sha256"]},
        "summary": summary,
        "function": {
            "ExecuteMapScript": addresses["ExecuteMapScript"],
            "rjt_cutsceneScriptCommands": table_address,
            **{row["name"]: row["address"] for row in handlers},
        },
        "dispatcher": {
            "address": table_address,
            "endExclusive": table_address + len(table_bytes),
            "sha256": hashlib.sha256(table_bytes).hexdigest().upper(),
            "targets": targets,
            "fillerTarget": "csc_doNothing",
            "fillerIndices": filler_indices,
            "sourceRomParity": True,
        },
        "familyCounts": dict(sorted(family_counts.items())),
        "abiFacts": {
            "commandWidthCounts": {
                str(width): count for width, count in sorted(command_width_counts.items())
            },
            "handlerFlowCounts": dict(sorted(handler_flow_counts.items())),
            "absoluteJumpHandlers": sorted(
                row["name"] for row in handlers if row["cursorFlow"] == "absolute-jump"
            ),
            "conditionalAbsoluteJumpHandlers": sorted(
                row["name"]
                for row in handlers
                if row["cursorFlow"] == "conditional-absolute-jump"
            ),
            "inlineActionProgramHandlers": sorted(
                row["name"]
                for row in handlers
                if row["cursorFlow"] == "inline-action-program"
            ),
            "shorthandOperands": [
                {
                    "macro": name,
                    "streamOffset": operand["streamOffset"],
                    "widthBytes": operand["widthBytes"],
                    "encoding": operand["encoding"],
                }
                for name, row in sorted(primary.items())
                for operand in row["operandLayout"]
                if operand["encoding"].startswith("shorthand:")
            ],
        },
        "macroContracts": {name: macros[name] for name in sorted(macros)},
        "macroSourceCounts": dict(sorted(source_counts.items())),
        "opcodeSourceCounts": opcode_counts,
        "unusedMacros": sorted(name for name in macros if source_counts[name] == 0),
        "handlers": handlers,
        "programCorpus": program_corpus,
        "dialogueCommandFacts": dialogue_command_facts,
        "transitionCommandFacts": transition_command_facts,
        "forceStateCommandFacts": force_state_command_facts,
        "storyStateCommandFacts": story_state_command_facts,
        "mapBlockMutationCommandFacts": map_block_mutation_command_facts,
        "entityPopulationCommandFacts": entity_population_command_facts,
        "mapLifecycleCommandFacts": map_lifecycle_command_facts,
        "runtimeQuestions": [
            "caller-dependent-story-branch-reachability-and-persistence",
            "entity-camera-text-wait-and-transition-frame-timing",
            "palette-fade-and-vdp-visible-presentation",
            *FORCE_STATE_RUNTIME_QUESTIONS,
            *ACTIVE_PARTY_RUNTIME_QUESTIONS,
            *STORY_STATE_RUNTIME_QUESTIONS,
            *MAP_BLOCK_MUTATION_RUNTIME_QUESTIONS,
            *ENTITY_POPULATION_RUNTIME_QUESTIONS,
            *MAP_LIFECYCLE_RUNTIME_QUESTIONS,
        ],
    }


def verify_map_script_engine_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map-script engine fixture")
    manifest = load_json(MANIFEST)
    output = build_map_script_engine_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-script engine static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["rom"]["sha256"]
        or any(output["function"][name] != address for name, address in fixture["function"].items())
    ):
        raise ValueError("map-script engine provenance/address drift")
    program_fields = {
        "programSummary": output["programCorpus"]["summary"],
        "transferCounts": output["programCorpus"]["transferCounts"],
        "referenceSummary": output["programCorpus"]["referenceSummary"],
        "unreferencedPrograms": output["programCorpus"]["unreferencedPrograms"],
        "sourceOnlyPrograms": output["programCorpus"]["sourceOnlyPrograms"],
        "largestPrograms": output["programCorpus"]["largestPrograms"],
        "storyStateSummary": output["programCorpus"]["storyState"]["summary"],
        "storyStateConstants": output["programCorpus"]["storyState"]["constants"],
        "storyReadFlagCounts": output["programCorpus"]["storyState"][
            "readFlagCounts"
        ],
        "storyReadWriteOverlapFlags": output["programCorpus"]["storyState"][
            "readWriteOverlapFlags"
        ],
        "storyDirectSetFlags": output["programCorpus"]["storyState"][
            "directSetFlags"
        ],
        "storyDirectClearFlags": output["programCorpus"]["storyState"][
            "directClearFlags"
        ],
        "storyBattleUnlockFlags": output["programCorpus"]["storyState"][
            "battleUnlockFlags"
        ],
        "dialogueCommandFacts": output["dialogueCommandFacts"],
        "transitionCommandFacts": output["transitionCommandFacts"],
        "forceStateCommandFacts": output["forceStateCommandFacts"],
        "storyStateCommandFacts": output["storyStateCommandFacts"],
        "mapBlockMutationCommandFacts": output["mapBlockMutationCommandFacts"],
        "entityPopulationCommandFacts": output["entityPopulationCommandFacts"],
        "mapLifecycleCommandFacts": output["mapLifecycleCommandFacts"],
    }
    for field in (
        "summary",
        "dispatcherFacts",
        "familyCounts",
        "abiFacts",
        "programSummary",
        "transferCounts",
        "referenceSummary",
        "unreferencedPrograms",
        "sourceOnlyPrograms",
        "largestPrograms",
        "storyStateSummary",
        "storyStateConstants",
        "storyReadFlagCounts",
        "storyReadWriteOverlapFlags",
        "storyDirectSetFlags",
        "storyDirectClearFlags",
        "storyBattleUnlockFlags",
        "dialogueCommandFacts",
        "transitionCommandFacts",
        "forceStateCommandFacts",
        "storyStateCommandFacts",
        "mapBlockMutationCommandFacts",
        "entityPopulationCommandFacts",
        "mapLifecycleCommandFacts",
        "mostUsedMacros",
        "unusedMacros",
        "runtimeQuestions",
    ):
        actual = program_fields.get(field)
        if field == "dispatcherFacts":
            actual = {
                "sha256": output["dispatcher"]["sha256"],
                "fillerTarget": output["dispatcher"]["fillerTarget"],
                "fillerIndices": output["dispatcher"]["fillerIndices"],
                "sourceRomParity": output["dispatcher"]["sourceRomParity"],
            }
        elif field == "mostUsedMacros":
            actual = [
                {"macro": name, "count": count}
                for name, count in sorted(
                    output["macroSourceCounts"].items(),
                    key=lambda item: (-item[1], item[0]),
                )[:12]
            ]
        elif field not in program_fields:
            actual = output[field]
        if fixture["expected"][field] != actual:
            raise ValueError(f"map-script engine {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map-script engine canonical output drift")
    destination = output_path or repo_path("local/derived/map-script-engine-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Slots": output["summary"]["dispatcherSlotCount"],
        "Handlers": output["summary"]["uniqueHandlerCount"],
        "Macros": output["summary"]["trackedMacroCount"],
        "SourceCommands": output["summary"]["sourceCommandCount"],
        "Status": "PASS",
    }
