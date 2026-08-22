from __future__ import annotations

import json
import re
from pathlib import Path
from typing import Any

from jsonschema import Draft7Validator

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

ID = "sf2-map3-optional-interactions-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map3-optional-interactions-static-v1.json")
SCHEMA = repo_path(
    "schemas/h2/map3-optional-interactions-static-fixture.schema.json"
)
TOOLCHAIN = repo_path("manifests/toolchain.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

MAP_SETUP_SOURCE = "data/maps/mapsetups.asm"
MAP3_ROOT = "data/maps/entries/map03"
MAP3_SETUP_ROOT = f"{MAP3_ROOT}/mapsetups"
DEFAULT_MAP3_SOURCE_PATHS = (
    MAP_SETUP_SOURCE,
    f"{MAP3_SETUP_ROOT}/pointertable.asm",
    f"{MAP3_SETUP_ROOT}/s1_entities.asm",
    f"{MAP3_SETUP_ROOT}/s2_entityevents.asm",
    f"{MAP3_SETUP_ROOT}/s4_descriptions.asm",
    f"{MAP3_SETUP_ROOT}/s5_itemevents.asm",
    f"{MAP3_ROOT}/7-chest-items.asm",
    f"{MAP3_ROOT}/8-other-items.asm",
)
GENERIC_SOURCE_PATHS = (
    "sf2mapsetupmacros.asm",
    "sf2mapmacros.asm",
    "code/common/scripting/map/mapsetupsfunctions_1.asm",
    "code/common/tech/jumpinterfaces/s05_jumpinterface.asm",
)

_LABEL = r"@?[A-Za-z_][A-Za-z0-9_]*"
_INSTRUCTION = re.compile(
    r"^\s*(?P<mnemonic>[A-Za-z][A-Za-z0-9_.]*)(?:\s+(?P<operands>.*?))?\s*$"
)


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Emit the one canonical UTF-8 representation used for this static fixture."""
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


def _disasm_root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    nested = root / "disasm"
    return nested if nested.is_dir() else root


def _source(root: Path, relative_path: str) -> str:
    path = root / relative_path
    if not path.is_file():
        raise ValueError(f"Map 3 optional-interactions source is missing: {relative_path}")
    return path.read_text(encoding="utf-8")


def _code_lines(source: str) -> list[tuple[int, str]]:
    """Return source lines with comments stripped; no comment can become an instruction."""
    return [
        (line_number, code.strip())
        for line_number, raw in enumerate(source.splitlines(), start=1)
        if (code := raw.split(";", maxsplit=1)[0].strip())
    ]


def _split_operands(operands: str) -> list[str]:
    values: list[str] = []
    start = 0
    depth = 0
    for index, character in enumerate(operands):
        if character == "(":
            depth += 1
        elif character == ")":
            depth -= 1
            if depth < 0:
                raise ValueError("Map 3 optional-interactions unbalanced operand parenthesis")
        elif character == "," and depth == 0:
            value = operands[start:index].strip()
            if not value:
                raise ValueError("Map 3 optional-interactions empty operand")
            values.append(value)
            start = index + 1
    if depth != 0:
        raise ValueError("Map 3 optional-interactions unbalanced operand parenthesis")
    value = operands[start:].strip()
    if value:
        values.append(value)
    elif operands.strip():
        raise ValueError("Map 3 optional-interactions empty final operand")
    return values


def _parse_integer(value: str, *, context: str) -> int:
    token = value.strip()
    if token.startswith("#"):
        token = token[1:]
    if token.startswith("$"):
        return int(token[1:], 16)
    if re.fullmatch(r"-?\d+", token):
        return int(token, 10)
    raise ValueError(f"{context}: expected a numeric source operand, got {value!r}")


def _macro_body(source: str, macro: str) -> list[str]:
    match = re.search(
        rf"(?ms)^\s*{re.escape(macro)}:\s*macro\s*$"
        rf"(?P<body>.*?)^\s*endm\s*$",
        source,
    )
    if match is None:
        raise ValueError(f"Map 3 optional-interactions macro is missing: {macro}")
    return [code for _, code in _code_lines(match.group("body"))]


def _require_macro_shape(source: str, macro: str, expected: list[str]) -> None:
    body = _macro_body(source, macro)
    if body != expected:
        raise ValueError(f"Map 3 optional-interactions macro shape drift: {macro}")


def _function_body(source: str, label: str) -> list[tuple[int, str]]:
    match = re.search(
        rf"(?ms)^\s*{re.escape(label)}:\s*$"
        rf"(?P<body>.*?)^\s*;\s*End of function\s+{re.escape(label)}\s*$",
        source,
    )
    if match is None:
        raise ValueError(f"Map 3 optional-interactions function is missing: {label}")
    start_line = source[: match.start("body")].count("\n") + 1
    return [
        (start_line + line_number - 1, code)
        for line_number, code in _code_lines(match.group("body"))
    ]


def _instruction_rows(lines: list[tuple[int, str]]) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    for line_number, code in lines:
        if re.fullmatch(rf"{_LABEL}:", code):
            continue
        match = _INSTRUCTION.fullmatch(code)
        if match is None:
            raise ValueError(
                f"Map 3 optional-interactions malformed instruction at line {line_number}: {code}"
            )
        rows.append(
            {
                "line": line_number,
                "mnemonic": match.group("mnemonic"),
                "operands": _split_operands(match.group("operands") or ""),
            }
        )
    return rows


def _require_row(
    rows: list[dict[str, Any]],
    *,
    mnemonic: str,
    operands: list[str],
    context: str,
) -> int:
    for index, row in enumerate(rows):
        if row["mnemonic"] == mnemonic and row["operands"] == operands:
            return index
    raise ValueError(f"Map 3 optional-interactions {context} source-use drift")


def _parse_pointer_setup(source: str, pointer_source: str) -> dict[str, Any]:
    map_setup_lines = _code_lines(source)
    target: str | None = None
    variant_flags: list[int] = []
    capture = False
    for _, code in map_setup_lines:
        match = re.fullmatch(
            rf"(?:{_LABEL}:\s+)?msMap\s+3\s*,\s*(?P<target>{_LABEL})", code
        )
        if match:
            target = match.group("target")
            capture = True
            continue
        if not capture:
            continue
        flag_match = re.fullmatch(rf"msFlag\s+(?P<flag>\d+)\s*,\s*{_LABEL}", code)
        if flag_match:
            variant_flags.append(int(flag_match.group("flag")))
            continue
        if code == "msMapEnd":
            break
        raise ValueError("Map 3 optional-interactions MapSetups row boundary drift")
    if target is None:
        raise ValueError("Map 3 optional-interactions default Map 3 setup row is missing")

    pointer_lines = _code_lines(pointer_source)
    try:
        start = next(
            index
            for index, (_, code) in enumerate(pointer_lines)
            if re.fullmatch(rf"{re.escape(target)}:\s*dc\.l\s+{_LABEL}", code)
        )
    except StopIteration as error:
        raise ValueError("Map 3 optional-interactions default pointer table is missing") from error
    first_match = re.fullmatch(
        rf"{re.escape(target)}:\s*dc\.l\s+(?P<slot>{_LABEL})", pointer_lines[start][1]
    )
    if first_match is None:
        raise AssertionError("pointer table match was not retained")
    slots = [first_match.group("slot")]
    for _, code in pointer_lines[start + 1 :]:
        match = re.fullmatch(rf"dc\.l\s+(?P<target>{_LABEL})", code)
        if match is None:
            break
        slots.append(match.group("target"))
    if not slots:
        raise ValueError("Map 3 optional-interactions pointer table has no slots")
    return {
        "mapId": 3,
        "defaultPointerTable": target,
        "variantFlagsInSourceOrder": variant_flags,
        "pointerSlotsInSourceOrder": slots,
    }


def _parse_entity_definitions(source: str) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    for line_number, code in _code_lines(source):
        match = re.fullmatch(
            r"(?P<macro>msFixedEntity|msWalkingEntity)\s+(?P<operands>.+)", code
        )
        if match is None:
            continue
        operands = _split_operands(match.group("operands"))
        macro = match.group("macro")
        expected_count = 5 if macro == "msFixedEntity" else 7
        if len(operands) != expected_count:
            raise ValueError("Map 3 optional-interactions entity record width drift")
        record: dict[str, Any] = {
            "recordIndex": len(rows),
            "sourceLine": line_number,
            "recordMacro": macro,
            "x": _parse_integer(operands[0], context="entity x"),
            "y": _parse_integer(operands[1], context="entity y"),
            "facing": operands[2],
            "mapSprite": operands[3],
        }
        if macro == "msFixedEntity":
            record["actionShape"] = {
                "kind": "fixed",
                "actionScript": operands[4],
            }
        else:
            record["actionShape"] = {
                "kind": "walking",
                "originX": _parse_integer(operands[4], context="walking origin x"),
                "originY": _parse_integer(operands[5], context="walking origin y"),
                "range": _parse_integer(operands[6], context="walking range"),
            }
        rows.append(record)
    if not rows:
        raise ValueError("Map 3 optional-interactions entity inventory is empty")
    return rows


def _event_program_body(source: str, target: str) -> list[tuple[int, str]]:
    if target == "Map3_DefaultEntityEvent":
        match = re.search(
            rf"(?ms)^\s*{re.escape(target)}:\s*$"
            r"(?P<body>.*?^\s*rts\s*$)",
            source,
        )
    else:
        match = re.search(
            rf"(?ms)^\s*{re.escape(target)}:\s*$"
            rf"(?P<body>.*?)^\s*;\s*End of function\s+{re.escape(target)}\s*$",
            source,
        )
    if match is None:
        raise ValueError(f"Map 3 optional-interactions event program is missing: {target}")
    start_line = source[: match.start("body")].count("\n") + 1
    return [
        (start_line + line_number - 1, code)
        for line_number, code in _code_lines(match.group("body"))
    ]


def _parse_event_program(source: str, target: str) -> dict[str, Any]:
    instructions = _instruction_rows(_event_program_body(source, target))
    if not instructions or instructions[-1]["mnemonic"] != "rts":
        raise ValueError(f"Map 3 optional-interactions event return drift: {target}")

    flag_conditions: list[dict[str, Any]] = []
    flag_effects: list[dict[str, Any]] = []
    text_indices: list[int] = []
    script_targets: list[dict[str, Any]] = []
    menu_call: dict[str, Any] | None = None
    for index, instruction in enumerate(instructions):
        mnemonic = instruction["mnemonic"]
        operands = instruction["operands"]
        if mnemonic == "chkFlg":
            if len(operands) != 1 or index + 1 >= len(instructions):
                raise ValueError(f"Map 3 optional-interactions flag-check shape drift: {target}")
            branch = instructions[index + 1]
            if not re.fullmatch(r"b(?:ne|eq)\.[bsw]", branch["mnemonic"]) or len(
                branch["operands"]
            ) != 1:
                raise ValueError(f"Map 3 optional-interactions flag branch drift: {target}")
            flag_conditions.append(
                {
                    "flag": _parse_integer(operands[0], context="checked flag"),
                    "operationIndex": index,
                    "branchMnemonic": branch["mnemonic"],
                    "branchTarget": branch["operands"][0],
                }
            )
        elif mnemonic == "setFlg":
            if len(operands) != 1:
                raise ValueError(f"Map 3 optional-interactions flag-set shape drift: {target}")
            flag_effects.append(
                {
                    "flag": _parse_integer(operands[0], context="set flag"),
                    "operationIndex": index,
                }
            )
        elif mnemonic == "txt":
            if len(operands) != 1:
                raise ValueError(f"Map 3 optional-interactions text shape drift: {target}")
            text_indices.append(_parse_integer(operands[0], context="text index"))
        elif mnemonic == "script":
            if len(operands) != 1 or not re.fullmatch(_LABEL, operands[0]):
                raise ValueError(f"Map 3 optional-interactions script shape drift: {target}")
            script_targets.append({"target": operands[0], "operationIndex": index})
        elif mnemonic == "jsr":
            if operands != ["j_ChurchMenu"]:
                raise ValueError(f"Map 3 optional-interactions menu-call target drift: {target}")
            if menu_call is not None:
                raise ValueError(f"Map 3 optional-interactions duplicate menu call: {target}")
            menu_call = {
                "instructionMnemonic": mnemonic,
                "instructionTarget": "j_ChurchMenu",
                "effectiveTarget": "ChurchMenu",
                "operationIndex": index,
            }

    return {
        "target": target,
        "operationOrder": [row["mnemonic"] for row in instructions],
        "textIndices": text_indices,
        "flagConditions": flag_conditions,
        "flagEffects": flag_effects,
        "scriptTargets": script_targets,
        "menuCall": menu_call,
    }


def _route_relevance(target: str) -> dict[str, str]:
    # These two labels are the only Map 3 entity-event entries callback-observed
    # by the accepted R2 opening.  All other records remain route-unknown rather
    # than being inferred optional from source placement or comments.
    if target in {"Map3_EntityEvent0", "Map3_EntityEvent15"}:
        return {
            "evidence": "Confirmed",
            "classification": "mandatory-observed-opening",
        }
    return {"evidence": "Unknown", "classification": "unknown"}


def _parse_entity_events(source: str) -> list[dict[str, Any]]:
    event_rows: list[dict[str, Any]] = []
    table_end = source.find("Map3_EntityEvent0:")
    if table_end == -1:
        raise ValueError("Map 3 optional-interactions entity event table boundary is missing")
    for line_number, code in _code_lines(source[:table_end]):
        match = re.fullmatch(r"msEntityEvent\s+(?P<operands>.+)", code)
        if match:
            operands = _split_operands(match.group("operands"))
            if len(operands) != 3:
                raise ValueError("Map 3 optional-interactions entity event record width drift")
            target_match = re.fullmatch(
                rf"(?P<target>{_LABEL})-ms_map3_EntityEvents", operands[2]
            )
            if target_match is None:
                raise ValueError("Map 3 optional-interactions entity event target relation drift")
            target = target_match.group("target")
            event_rows.append(
                {
                    "recordIndex": len(event_rows),
                    "sourceLine": line_number,
                    "recordMacro": "msEntityEvent",
                    "entityId": operands[0],
                    "facing": operands[1],
                    "program": _parse_event_program(source, target),
                    "routeRelevance": _route_relevance(target),
                }
            )
            continue
        default_match = re.fullmatch(r"msDefaultEntityEvent\s+(?P<operand>.+)", code)
        if default_match:
            target_match = re.fullmatch(
                rf"(?P<target>{_LABEL})-ms_map3_EntityEvents", default_match.group("operand")
            )
            if target_match is None:
                raise ValueError("Map 3 optional-interactions default entity target drift")
            target = target_match.group("target")
            event_rows.append(
                {
                    "recordIndex": len(event_rows),
                    "sourceLine": line_number,
                    "recordMacro": "msDefaultEntityEvent",
                    "entityId": "$FD",
                    "facing": "0",
                    "program": _parse_event_program(source, target),
                    "routeRelevance": _route_relevance(target),
                }
            )
    if not event_rows or event_rows[-1]["recordMacro"] != "msDefaultEntityEvent":
        raise ValueError("Map 3 optional-interactions default entity route is missing")
    return event_rows


def _parse_area_description_base(source: str) -> int:
    lines = _code_lines(source)
    try:
        index = next(
            index for index, (_, code) in enumerate(lines) if code == "ms_map3_AreaDescriptions:"
        )
    except StopIteration as error:
        raise ValueError("Map 3 optional-interactions area-description entry is missing") from error
    for _, code in lines[index + 1 :]:
        match = re.fullmatch(r"move\.w\s+(?P<base>#[\$0-9A-Fa-f]+)\s*,\s*d3", code)
        if match:
            return _parse_integer(match.group("base"), context="area description base")
        if re.fullmatch(rf"{_LABEL}:", code):
            break
    raise ValueError("Map 3 optional-interactions area-description base use-site is missing")


def _parse_area_descriptions(
    source: str, first_text_base: int
) -> list[dict[str, Any]]:
    second_text_base = _parse_area_description_base(source)
    rows: list[dict[str, Any]] = []
    for line_number, code in _code_lines(source):
        match = re.fullmatch(
            rf"(?:{_LABEL}:\s+)?msDesc\s+(?P<operands>.+)", code
        )
        if match is None:
            continue
        operands = _split_operands(match.group("operands"))
        if len(operands) != 4:
            raise ValueError("Map 3 optional-interactions area-description record width drift")
        interaction_kind = _parse_integer(operands[2], context="area interaction kind")
        second_offset = _parse_integer(operands[3], context="area second text offset")
        rows.append(
            {
                "recordIndex": len(rows),
                "sourceLine": line_number,
                "recordMacro": "msDesc",
                "x": _parse_integer(operands[0], context="area x"),
                "y": _parse_integer(operands[1], context="area y"),
                "facingConstraint": "not-stored",
                "interactionKind": interaction_kind,
                "firstTextIndex": first_text_base + interaction_kind,
                "secondTextIndex": second_text_base + second_offset,
                "effectShape": "two-display-text-calls",
                "routeRelevance": {"evidence": "Unknown", "classification": "unknown"},
            }
        )
    if not rows:
        raise ValueError("Map 3 optional-interactions area-description inventory is empty")
    return rows


def _parse_item_placements(
    source: str, *, source_kind: str, source_path: str
) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    saw_end = False
    for line_number, code in _code_lines(source):
        match = re.fullmatch(r"mapItem\s+(?P<operands>.+)", code)
        if match:
            operands = _split_operands(match.group("operands"))
            if len(operands) != 4:
                raise ValueError("Map 3 optional-interactions item placement width drift")
            rows.append(
                {
                    "sourceKind": source_kind,
                    "sourcePath": source_path,
                    "sourceLine": line_number,
                    "recordMacro": "mapItem",
                    "x": _parse_integer(operands[0], context="item x"),
                    "y": _parse_integer(operands[1], context="item y"),
                    "associatedFlag": _parse_integer(operands[2], context="item flag"),
                    "itemIdentifier": operands[3],
                    "routeRelevance": {"evidence": "Unknown", "classification": "unknown"},
                }
            )
        elif code == "endWord":
            saw_end = True
    if not saw_end:
        raise ValueError("Map 3 optional-interactions item placement terminator is missing")
    if len(rows) != 1:
        raise ValueError("Map 3 optional-interactions item placement cardinality drift")
    return rows


def _parse_generic_shapes(
    setup_macros: str, map_macros: str, dispatch_source: str, jump_interfaces: str
) -> dict[str, Any]:
    _require_macro_shape(
        setup_macros,
        "msEntityEvent",
        ["dc.b \\1", "dc.b \\2", "dc.w \\3"],
    )
    _require_macro_shape(
        setup_macros,
        "msDefaultEntityEvent",
        ["dc.b $FD", "dc.b 0", "dc.w \\1"],
    )
    _require_macro_shape(
        setup_macros,
        "msDesc",
        ["dc.b \\1", "dc.b \\2", "dc.b 0", "dc.b 0", "dc.b \\3", "dc.b \\4"],
    )
    _require_macro_shape(
        setup_macros,
        "msDefaultItemEvent",
        ["dc.l $FD000000", "dc.w \\1"],
    )
    _require_macro_shape(
        map_macros,
        "mapItem",
        ["dc.b \\1", "dc.b \\2", "dc.b \\3", "defineShorthand.b ITEM_,\\4"],
    )
    _require_macro_shape(map_macros, "endWord", ["dc.w $FFFF"])

    entity_rows = _instruction_rows(_function_body(dispatch_source, "RunMapSetupEntityEvent"))
    item_rows = _instruction_rows(_function_body(dispatch_source, "RunMapSetupItemEvent"))
    area_rows = _instruction_rows(_function_body(dispatch_source, "DisplayAreaDescription"))
    _require_row(
        entity_rows,
        mnemonic="cmpi.b",
        operands=["#$FD", "(a0,d7.w)"],
        context="entity default selector",
    )
    _require_row(
        entity_rows,
        mnemonic="adda.w",
        operands=["2(a0,d7.w)", "a0"],
        context="entity default target offset",
    )
    _require_row(
        entity_rows,
        mnemonic="addq.w",
        operands=["#4", "d7"],
        context="entity record stride",
    )
    _require_row(
        entity_rows,
        mnemonic="jsr",
        operands=["(a0)"],
        context="entity event dispatch",
    )
    _require_row(
        item_rows,
        mnemonic="cmpi.b",
        operands=["#$FD", "(a0,d7.w)"],
        context="item default selector",
    )
    _require_row(
        item_rows,
        mnemonic="adda.w",
        operands=["4(a0,d7.w)", "a0"],
        context="item default target offset",
    )
    _require_row(
        item_rows,
        mnemonic="addq.w",
        operands=["#6", "d7"],
        context="item record stride",
    )
    _require_row(
        item_rows,
        mnemonic="jsr",
        operands=["(a0)"],
        context="item event dispatch",
    )
    _require_row(
        area_rows,
        mnemonic="addi.w",
        operands=["#423", "d0"],
        context="area first text base",
    )
    _require_row(
        area_rows,
        mnemonic="addq.w",
        operands=["#6", "d7"],
        context="area record stride",
    )
    if sum(
        row["mnemonic"] == "jsr" and row["operands"] == ["(DisplayText).w"]
        for row in area_rows
    ) != 2:
        raise ValueError("Map 3 optional-interactions area display call shape drift")

    alias_match = re.search(
        r"(?ms)^\s*j_ChurchMenu:\s*$\s*^\s*jmp\s+(?P<target>ChurchMenu)\(pc\)\s*$",
        jump_interfaces,
    )
    if alias_match is None:
        raise ValueError("Map 3 optional-interactions Church menu alias drift")
    return {
        "entityEvent": {
            "recordStrideBytes": 4,
            "defaultSelectorByte": "$FD",
            "defaultTargetOffsetBytes": 2,
            "dispatch": "jsr-(a0)",
        },
        "itemEvent": {
            "recordStrideBytes": 6,
            "defaultSelectorByte": "$FD",
            "defaultTargetOffsetBytes": 4,
            "dispatch": "jsr-(a0)",
        },
        "areaDescription": {
            "recordStrideBytes": 6,
            "firstTextBase": 423,
            "displayTextCallCount": 2,
        },
        "menuAlias": {
            "instructionTarget": "j_ChurchMenu",
            "effectiveTarget": alias_match.group("target"),
        },
    }


def _parse_default_item_event(source: str, shapes: dict[str, Any]) -> dict[str, Any]:
    lines = _code_lines(source)
    record_match: re.Match[str] | None = None
    for _, code in lines:
        match = re.fullmatch(r"msDefaultItemEvent\s+(?P<operand>.+)", code)
        if match:
            record_match = match
            break
    if record_match is None:
        raise ValueError("Map 3 optional-interactions default item event record is missing")
    target_match = re.fullmatch(
        rf"(?P<target>{_LABEL})-ms_map3_Section5", record_match.group("operand")
    )
    if target_match is None:
        raise ValueError("Map 3 optional-interactions default item event target relation drift")
    target = target_match.group("target")
    instructions = _instruction_rows(
        _function_body(source, target)
    )
    if [row["mnemonic"] for row in instructions] != ["rts"]:
        raise ValueError("Map 3 optional-interactions default item event is not a direct return")
    return {
        "recordMacro": "msDefaultItemEvent",
        "target": target,
        "targetOperationOrder": ["rts"],
        "dispatchShape": shapes["itemEvent"],
    }


def _summary(
    *,
    entity_definitions: list[dict[str, Any]],
    entity_events: list[dict[str, Any]],
    area_descriptions: list[dict[str, Any]],
    item_placements: list[dict[str, Any]],
) -> dict[str, int]:
    return {
        "sourcePathCount": len(DEFAULT_MAP3_SOURCE_PATHS) + len(GENERIC_SOURCE_PATHS),
        "defaultMap3SourcePathCount": len(DEFAULT_MAP3_SOURCE_PATHS),
        "genericSourcePathCount": len(GENERIC_SOURCE_PATHS),
        "entityDefinitionCount": len(entity_definitions),
        "entityEventRouteCount": len(entity_events),
        "mandatoryObservedOpeningRouteCount": sum(
            row["routeRelevance"]["classification"] == "mandatory-observed-opening"
            for row in entity_events
        ),
        "unknownEntityEventRouteCount": sum(
            row["routeRelevance"]["classification"] == "unknown" for row in entity_events
        ),
        "areaDescriptionCount": len(area_descriptions),
        "itemPlacementCount": len(item_placements),
    }


def _project_entity_definitions(rows: list[dict[str, Any]]) -> dict[str, Any]:
    return {
        "sourcePath": f"{MAP3_SETUP_ROOT}/s1_entities.asm",
        "recordCount": len(rows),
        "recordMacroCounts": {
            "msFixedEntity": sum(row["recordMacro"] == "msFixedEntity" for row in rows),
            "msWalkingEntity": sum(
                row["recordMacro"] == "msWalkingEntity" for row in rows
            ),
        },
        "actionKindCounts": {
            "fixed": sum(row["actionShape"]["kind"] == "fixed" for row in rows),
            "walking": sum(row["actionShape"]["kind"] == "walking" for row in rows),
        },
    }


def _project_area_descriptions(rows: list[dict[str, Any]]) -> dict[str, Any]:
    return {
        "sourcePath": f"{MAP3_SETUP_ROOT}/s4_descriptions.asm",
        "recordCount": len(rows),
        "recordMacro": "msDesc",
        "recordStrideBytes": 6,
        "effectShape": "two-display-text-calls",
        "routeRelevanceCounts": {
            "unknown": sum(
                row["routeRelevance"]["classification"] == "unknown" for row in rows
            )
        },
    }


def _project_item_placements(rows: list[dict[str, Any]]) -> dict[str, Any]:
    source_kinds = ("chest", "other")
    source_paths = {
        "chest": f"{MAP3_ROOT}/7-chest-items.asm",
        "other": f"{MAP3_ROOT}/8-other-items.asm",
    }
    return {
        "recordCount": len(rows),
        "sourceOwners": [
            {
                "sourcePath": source_paths[kind],
                "sourceKind": kind,
                "recordCount": sum(row["sourceKind"] == kind for row in rows),
            }
            for kind in source_kinds
        ],
        "recordMacro": "mapItem",
        "terminatorMacro": "endWord",
        "unknownRouteRelevanceCount": sum(
            row["routeRelevance"]["classification"] == "unknown" for row in rows
        ),
    }


def _structural_schema() -> dict[str, Any]:
    """Use the reusable record contract when parsing synthetic or drifted source."""
    schema = load_json(SCHEMA)
    fixture_schema = schema.get("definitions", {}).get("fixture")
    if not isinstance(fixture_schema, dict):
        raise ValueError("Map 3 optional-interactions structural schema is missing")
    schema.pop("allOf", None)
    schema["$ref"] = "#/definitions/fixture"
    return schema


def _validate_structural_output(output: dict[str, Any]) -> None:
    errors = sorted(
        Draft7Validator(_structural_schema()).iter_errors(output),
        key=lambda error: list(error.absolute_path),
    )
    if errors:
        location = ".".join(str(part) for part in errors[0].absolute_path) or "<root>"
        raise ValueError(
            "Map 3 optional-interactions static output failed structural schema validation: "
            f"{location}: {errors[0].message}"
        )


def build_map3_optional_interactions(upstream_path: Path) -> dict[str, Any]:
    """Parse the complete default Map 3 optional-interaction source surface.

    The argument may name either the upstream checkout or its ``disasm`` directory.
    The parser never reads a ROM, a generated artifact, or any dialogue payload.
    """
    root = _disasm_root(upstream_path)
    sources = {
        path: _source(root, path)
        for path in (*DEFAULT_MAP3_SOURCE_PATHS, *GENERIC_SOURCE_PATHS)
    }
    shapes = _parse_generic_shapes(
        sources["sf2mapsetupmacros.asm"],
        sources["sf2mapmacros.asm"],
        sources["code/common/scripting/map/mapsetupsfunctions_1.asm"],
        sources["code/common/tech/jumpinterfaces/s05_jumpinterface.asm"],
    )
    pointer_setup = _parse_pointer_setup(
        sources[MAP_SETUP_SOURCE], sources[f"{MAP3_SETUP_ROOT}/pointertable.asm"]
    )
    entity_definitions = _parse_entity_definitions(
        sources[f"{MAP3_SETUP_ROOT}/s1_entities.asm"]
    )
    entity_events = _parse_entity_events(
        sources[f"{MAP3_SETUP_ROOT}/s2_entityevents.asm"]
    )
    area_descriptions = _parse_area_descriptions(
        sources[f"{MAP3_SETUP_ROOT}/s4_descriptions.asm"],
        shapes["areaDescription"]["firstTextBase"],
    )
    item_placements = [
        *_parse_item_placements(
            sources[f"{MAP3_ROOT}/7-chest-items.asm"],
            source_kind="chest",
            source_path=f"{MAP3_ROOT}/7-chest-items.asm",
        ),
        *_parse_item_placements(
            sources[f"{MAP3_ROOT}/8-other-items.asm"],
            source_kind="other",
            source_path=f"{MAP3_ROOT}/8-other-items.asm",
        ),
    ]
    default_item_event = _parse_default_item_event(
        sources[f"{MAP3_SETUP_ROOT}/s5_itemevents.asm"],
        shapes,
    )
    toolchain = load_json(TOOLCHAIN)
    output = {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": toolchain["sf2disasm"]["commit"],
        },
        "romSha256": load_json(ROM_MANIFEST)["hashes"]["sha256"],
        "sourceSurface": {
            "defaultMap3Paths": list(DEFAULT_MAP3_SOURCE_PATHS),
            "genericConsumerPaths": list(GENERIC_SOURCE_PATHS),
        },
        "pointerSetup": pointer_setup,
        "genericShapes": shapes,
        "entityDefinitions": _project_entity_definitions(entity_definitions),
        "entityEventRoutes": entity_events,
        "areaDescriptions": _project_area_descriptions(area_descriptions),
        "itemPlacements": _project_item_placements(item_placements),
        "defaultItemEvent": default_item_event,
        "acceptedMainCrossChecks": [
            "sf2-map-data-static-v1",
            "sf2-map-events-static-v1",
            "sf2-map3-battle01-natural-route-runtime-v1",
            "sf2-map3-messenger-acceptance-runtime-v1",
        ],
        "runtimeQuestionGroups": [
            "map3-optional-interactions/reachability-and-route-relevance",
            "map3-optional-interactions/flag-dependent-state-and-effects",
            "map3-optional-interactions/rendered-dialogue-menu-presentation-and-timing",
            "map3-optional-interactions/item-and-default-dispatch-outcomes",
        ],
        "summary": _summary(
            entity_definitions=entity_definitions,
            entity_events=entity_events,
            area_descriptions=area_descriptions,
            item_placements=item_placements,
        ),
    }
    _validate_structural_output(output)
    return output


def verify_map3_optional_interactions(upstream_path: Path) -> dict[str, Any]:
    """Validate the checked-in semantic fixture against a fresh source parse."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    output = build_map3_optional_interactions(upstream_path)
    if output != fixture:
        raise ValueError("Map 3 optional-interactions complete semantic fixture drift")
    return output
