"""Public static H2 contract for exploration-field item effects."""

from __future__ import annotations

import hashlib
import json
import re
import subprocess
from pathlib import Path
from typing import Any

from jsonschema import Draft7Validator

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom

ID = "sf2-field-item-effects-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/field-item-effects-static-v1.json")
SCHEMA = repo_path("schemas/h2/field-item-effects-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")

_LISTING = Path("build/sf2build-h1.lst")
_H1_BINARY = Path("build/sf2build-h1.bin")
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"

_SOURCE_SURFACE = (
    "sf2const.asm",
    "sf2enums.asm",
    "layout/sf2-05-0x020000-0x028000.asm",
    "code/common/menus/main/mainactions.asm",
    "code/common/menus/caravan/caravanactions_1.asm",
    "code/common/menus/item/isitemusableonfield.asm",
    "data/stats/items/usableoutsidebattleitems.asm",
    "code/common/menus/item/fielditemeffects.asm",
)
_SOURCE_SHA256 = dict(
    zip(
        _SOURCE_SURFACE,
        (
            "17738776F811F66322F278CFBF10E8B376612F4F9EDFC2C7AA0A5DB81CDFB3FB",
            "ABA0DFEE4F4D3BFCD49646C03B5229DD6458FFC3A90174907823D710884020EE",
            "F5FD087710CE328C5D3DD8ADBCE2CDFD8D60F4F850743BA020F69EC56E5D63C6",
            "E69B3D40DF6658CF761A035FA1B6643DD9770B3C82F4CBB21D5AC4D3A50F2777",
            "E2C13080AC8755707248724DFD0845A91BAC793A763817ABF46DCEB129E2ACA6",
            "86EEF4CD2A87BEDD18A705058BFA9C44E3D6E5067E43E24833DE249C7B6CD67A",
            "303CF86BFBEA8F355E5121F9325161A84BDB090B74563772CDF90536A2D2A075",
            "02F80F3670229417A799E95A802B3AD19F71A0C85317BE92679CAFB8593D4CB5",
        ),
        strict=True,
    )
)

_ANCHORS = (
    ("callers.fieldMenu", 0x2157C, 4),
    ("callers.caravan", 0x225D0, 4),
    ("usability.function", 0x229CA, 24),
    ("usability.table", 0x229E2, 10),
    ("dispatch.functionAndTable", 0x229EC, 98),
    ("effects.curePoison", 0x22A4E, 34),
    ("effects.curePoisonAndParalysis", 0x22A70, 62),
    ("effects.increaseAtt", 0x22AAE, 40),
    ("effects.increaseDef", 0x22AD6, 40),
    ("effects.increaseAgi", 0x22AFE, 40),
    ("effects.increaseMov", 0x22B26, 60),
    ("effects.increaseHp", 0x22B62, 40),
    ("effects.increaseMp", 0x22B8A, 56),
    ("effects.levelUp", 0x22BC2, 158),
)

_FUNCTION_ADDRESSES = {
    "FieldMenuCall": 0x2157C,
    "CaravanCall": 0x225D0,
    "IsItemUsableOnField": 0x229CA,
    "table_UsableOnFieldItems": 0x229E2,
    "UseItemOnField": 0x229EC,
    "rjt_FieldItemEffects": 0x22A22,
    "fieldItem_CurePoison": 0x22A4E,
    "fieldItem_CurePoisonAndParalysis": 0x22A70,
    "fieldItem_IncreaseAtt": 0x22AAE,
    "fieldItem_IncreaseDef": 0x22AD6,
    "fieldItem_IncreaseAgi": 0x22AFE,
    "fieldItem_IncreaseMov": 0x22B26,
    "fieldItem_IncreaseHp": 0x22B62,
    "fieldItem_IncreaseMp": 0x22B8A,
    "fieldItem_LevelUp": 0x22BC2,
    "GenerateRandomNumber": 0x1600,
    "GetStatusEffects": 0x8426,
    "UpdateCombatantStats": 0x89CE,
    "LevelUp": 0x9484,
    "LEVELUP_ARGUMENTS": 0xFFAF82,
}

_ITEM_SYMBOLS = (
    "ITEM_ANTIDOTE",
    "ITEM_FAIRY_POWDER",
    "ITEM_POWER_WATER",
    "ITEM_PROTECT_MILK",
    "ITEM_QUICK_CHICKEN",
    "ITEM_RUNNING_PIMENTO",
    "ITEM_CHEERFUL_BREAD",
    "ITEM_BRIGHT_HONEY",
    "ITEM_BRAVE_APPLE",
)
_ITEM_IDS = (3, 5, 9, 10, 11, 12, 13, 14, 15)
_DISPATCH_EFFECT_LABELS = {
    "fieldItem_CurePoison": "curePoison",
    "fieldItem_CurePoisonAndParalysis": "curePoisonAndParalysis",
    "fieldItem_IncreaseAtt": "increaseAtt",
    "fieldItem_IncreaseDef": "increaseDef",
    "fieldItem_IncreaseAgi": "increaseAgi",
    "fieldItem_IncreaseMov": "increaseMov",
    "fieldItem_IncreaseHp": "increaseHp",
    "fieldItem_IncreaseMp": "increaseMp",
    "fieldItem_LevelUp": "levelUp",
}
_EFFECT_TEXT_IDS = {
    "curePoison": [149, 148],
    "curePoisonAndParalysis": [149, 156, 148],
    "increaseAtt": [150],
    "increaseDef": [151],
    "increaseAgi": [152],
    "increaseMov": [153],
    "increaseHp": [154],
    "increaseMp": [155, 148],
    "levelUp": [148, 244, 266, 267, 268, 269, 270, 271, 272, 3523],
}
_UNKNOWN_KEYS = (
    "natural-story-field-item-use-reachability",
    "caller-entry-state",
    "selected-item-member-slot",
    "actual-dispatch-target",
    "actual-status-clear-result",
    "actual-random-gain",
    "actual-movement-cap-result",
    "actual-zero-mp-rejection",
    "actual-level-up-result-and-spell-message-branch",
    "caller-item-removal-and-return",
    "persistence-across-map-save-story",
    "input-text-window-audio-vint-rendering",
)
_RETAINED_OWNER_PATHS = {
    "commonMenus": "tests/fixtures/h2/common-menus-static-v1.json",
    "fieldMenuControl": "tests/fixtures/h2/field-menu-control-static-v1.json",
    "coreStatsData": "tests/fixtures/h2/core-stats-data-static-v1.json",
    "itemAuxiliary": "tests/fixtures/h2/item-auxiliary-static-v1.json",
    "techInterfaces": "tests/fixtures/h2/tech-interfaces-static-v1.json",
    "commonStats": "tests/fixtures/h2/common-stats-static-v1.json",
    "rng": "tests/fixtures/h3/rng-v1.json",
    "levelUp": "tests/fixtures/h3/level-up-v1.json",
    "updateCombatantStats": "tests/fixtures/h3/level-up-refresh-v1.json",
}


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Emit the one canonical UTF-8 public-fixture representation."""
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8") + b"\n"


def _disasm_root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _without_comments(source: str) -> str:
    return "\n".join(line.split(";", maxsplit=1)[0].rstrip() for line in source.splitlines())


def _normalized(source: str) -> str:
    return "\n".join(
        re.sub(r"\s*,\s*", ",", " ".join(line.split()))
        for line in _without_comments(source).splitlines()
    )


def _require_order(source: str, expected: tuple[str, ...], context: str) -> None:
    clean = _normalized(source)
    cursor = 0
    for fragment in expected:
        found = clean.find(fragment, cursor)
        if found < 0:
            raise ValueError(f"Field item effects {context} source-use drift: {fragment}")
        cursor = found + len(fragment)


def _source_region(source: str, start: str, end: str, context: str) -> str:
    start_at = source.find(start)
    if start_at < 0:
        raise ValueError(f"Field item effects {context} start drift: {start}")
    end_at = source.find(end, start_at + len(start))
    if end_at < 0:
        raise ValueError(f"Field item effects {context} end drift: {end}")
    return source[start_at:end_at]


def _parse_equ(source: str) -> dict[str, int]:
    values: dict[str, int] = {}
    for raw in _without_comments(source).splitlines():
        match = re.match(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*:?\s+equ\s+([^\s]+)", raw)
        if match is None:
            continue
        literal = match.group(2)
        if literal.startswith("$"):
            values[match.group(1)] = int(literal[1:], 16)
        elif literal.lstrip("-").isdigit():
            values[match.group(1)] = int(literal)
    return values


def _direct_calls(source: str, target: str) -> list[str]:
    calls: list[str] = []
    pattern = re.compile(rf"^\s*bsr\.w\s+{re.escape(target)}\s*$")
    for raw in _without_comments(source).splitlines():
        if pattern.match(raw):
            calls.append(target)
    return calls


def _caller_paths_from_source_map(sources: dict[str, str]) -> list[str]:
    """Return the complete sorted direct-call inventory for the exact instruction form."""
    paths = [
        path for path, source in sorted(sources.items()) if _direct_calls(source, "UseItemOnField")
    ]
    return paths


def _complete_code_caller_paths(disasm_root: Path) -> list[str]:
    code_root = disasm_root / "code"
    if not code_root.is_dir():
        raise ValueError("Field item effects complete code source is missing")
    sources = {
        path.relative_to(disasm_root).as_posix(): path.read_bytes().decode("latin-1")
        for path in code_root.rglob("*.asm")
    }
    paths = _caller_paths_from_source_map(sources)
    expected = [
        "code/common/menus/caravan/caravanactions_1.asm",
        "code/common/menus/main/mainactions.asm",
    ]
    if paths != expected:
        raise ValueError("Field item effects complete direct caller inventory drift")
    site_counts = {path: len(_direct_calls(sources[path], "UseItemOnField")) for path in paths}
    if site_counts != {path: 1 for path in expected}:
        raise ValueError("Field item effects complete direct caller site-count drift")
    return paths


def _read_source_surface(root: Path) -> tuple[dict[str, str], list[dict[str, str]]]:
    text: dict[str, str] = {}
    identities: list[dict[str, str]] = []
    for relative in _SOURCE_SURFACE:
        path = root / relative
        if not path.is_file():
            raise ValueError(f"Field item effects source is missing: {relative}")
        data = path.read_bytes()
        sha256 = hashlib.sha256(data).hexdigest().upper()
        if sha256 != _SOURCE_SHA256[relative]:
            raise ValueError(f"Field item effects source hash drift: {relative}")
        identities.append({"path": relative, "sha256": sha256})
        text[relative] = data.decode("utf-8").replace("\r\n", "\n")
    if len(identities) != 8:
        raise ValueError("Field item effects source denominator drift")
    return text, identities


def _anchor_projection(h1_binary: bytes, rom: bytes) -> list[dict[str, int | str]]:
    anchors: list[dict[str, int | str]] = []
    for identifier, address, width in _ANCHORS:
        h1 = h1_binary[address : address + width]
        rom_bytes = rom[address : address + width]
        if len(h1) != width or len(rom_bytes) != width or h1 != rom_bytes:
            raise ValueError(f"Field item effects H1/ROM anchor drift: {identifier}")
        anchors.append(
            {
                "id": identifier,
                "address": address,
                "endAddressExclusive": address + width,
                "byteLength": width,
            }
        )
    if len(anchors) != 14:
        raise ValueError("Field item effects H1/ROM anchor denominator drift")
    return anchors


def _retained_owners() -> dict[str, dict[str, str]]:
    owners: dict[str, dict[str, str]] = {}
    for name, relative in _RETAINED_OWNER_PATHS.items():
        path = repo_path(relative)
        fixture = load_json(path)
        fixture_id = fixture.get("id")
        if not isinstance(fixture_id, str):
            raise ValueError(f"Field item effects retained owner identity drift: {relative}")
        owners[name] = {
            "fixtureId": fixture_id,
            "fixtureSha256": hashlib.sha256(path.read_bytes()).hexdigest().upper(),
        }
    if len(owners) != 9:
        raise ValueError("Field item effects retained owner denominator drift")
    return owners


def _text_ids(source: str, start: str, end: str, context: str) -> list[int]:
    region = _source_region(source, start, end, context)
    values = [
        int(value)
        for value in re.findall(r"^\s*txt\s+(\d+)\b", _without_comments(region), re.MULTILINE)
    ]
    if not values:
        raise ValueError(f"Field item effects {context} text-ID drift")
    return list(dict.fromkeys(values))


def _exact_text_ids(source: str, start: str, end: str, effect: str) -> list[int]:
    values = _text_ids(source, start, end, effect)
    if values != _EFFECT_TEXT_IDS[effect]:
        raise ValueError(f"Field item effects {effect} public text-ID drift")
    return values


def _parse_usable_item_table(source: str) -> tuple[list[int], int]:
    lines = [line.strip() for line in _without_comments(source).splitlines() if line.strip()]
    try:
        table_start = lines.index("table_UsableOnFieldItems:")
    except ValueError as exc:
        raise ValueError("Field item effects usability table label drift") from exc
    expected_rows = [
        *(f"item {symbol.removeprefix('ITEM_')}" for symbol in _ITEM_SYMBOLS),
        "tableEnd.b",
    ]
    if lines[table_start + 1 :] != expected_rows:
        raise ValueError("Field item effects usability table rows or sentinel drift")
    return list(_ITEM_IDS), 0xFF


def _parse_word_literal(value: str) -> int:
    return int(value[1:], 16) if value.startswith("$") else int(value)


def _parse_dispatch_order(source: str) -> list[dict[str, int | str]]:
    region = _source_region(source, "rjt_FieldItemEffects:", "@Done:", "dispatch table")
    lines = [line.strip() for line in _without_comments(region).splitlines() if line.strip()]
    if not lines or lines.pop(0) != "rjt_FieldItemEffects:":
        raise ValueError("Field item effects dispatch table label drift")
    rows: list[tuple[int, str]] = []
    cursor = 0
    while cursor < len(lines):
        if lines[cursor] == "dc.w $FFFF":
            if cursor != len(lines) - 1:
                raise ValueError("Field item effects dispatch terminal is not alone")
            break
        item_match = re.fullmatch(r"dc\.w\s+(\$[0-9A-F]+|\d+)", lines[cursor])
        if item_match is None:
            raise ValueError("Field item effects dispatch item row drift")
        item_id = _parse_word_literal(item_match.group(1))
        if item_id == 0xFFFF or cursor + 1 >= len(lines):
            raise ValueError("Field item effects dispatch terminal or offset drift")
        offset_match = re.fullmatch(
            r"dc\.w\s+(fieldItem_[A-Za-z0-9_]+)-rjt_FieldItemEffects", lines[cursor + 1]
        )
        if offset_match is None:
            raise ValueError("Field item effects dispatch offset drift")
        rows.append((item_id, offset_match.group(1)))
        cursor += 2
    else:
        raise ValueError("Field item effects dispatch terminal is missing")

    labels = [label for _, label in rows]
    expected_labels = list(_DISPATCH_EFFECT_LABELS)
    if len(rows) != len(_ITEM_IDS) or len(set(labels)) != len(labels):
        raise ValueError("Field item effects dispatch row denominator or duplicate offset drift")
    if [item_id for item_id, _ in rows] != list(_ITEM_IDS):
        raise ValueError("Field item effects dispatch item order drift")
    if labels != expected_labels:
        raise ValueError("Field item effects dispatch effect-label order drift")
    return [
        {"itemId": item_id, "effect": _DISPATCH_EFFECT_LABELS[label]} for item_id, label in rows
    ]


def _validate_source_contract(
    text: dict[str, str], *, complete_caller_paths: list[str] | None = None
) -> dict[str, Any]:
    """Parse only source-backed field-item structure and use-site semantics."""
    constants = _parse_equ(text["sf2const.asm"])
    constants.update(_parse_equ(text["sf2enums.asm"]))
    required_constants = {
        "ITEMENTRY_MASK_INDEX": 0x7F,
        "STATUSEFFECT_BIT_STUN": 0,
        "STATUSEFFECT_BIT_POISON": 1,
        "STATUSEFFECT_STUN": 1,
        "STATUSEFFECT_POISON": 2,
        "LEVELUP_ARGUMENTS": 0xFFAF82,
        **{symbol: value for symbol, value in zip(_ITEM_SYMBOLS, _ITEM_IDS, strict=True)},
    }
    if {key: constants.get(key) for key in required_constants} != required_constants:
        raise ValueError("Field item effects authoritative constants drift")

    layout = text["layout/sf2-05-0x020000-0x028000.asm"]
    canonical_includes = (
        "code\\common\\menus\\item\\isitemusableonfield.asm",
        "data\\stats\\items\\usableoutsidebattleitems.asm",
        "code\\common\\menus\\item\\fielditemeffects.asm",
    )
    _require_order(layout, canonical_includes, "canonical layout includes")
    if "code\\common\\stats\\items\\fielditemeffects.asm" in _without_comments(layout):
        raise ValueError("Field item effects alternate layout exclusion drift")

    call_sites = (
        (
            "fieldMenu",
            "code/common/menus/main/mainactions.asm",
            _FUNCTION_ADDRESSES["FieldMenuCall"],
        ),
        (
            "caravan",
            "code/common/menus/caravan/caravanactions_1.asm",
            _FUNCTION_ADDRESSES["CaravanCall"],
        ),
    )
    callers: dict[str, dict[str, Any]] = {}
    total_calls = 0
    for name, path, address in call_sites:
        matches = _direct_calls(text[path], "UseItemOnField")
        if len(matches) != 1:
            raise ValueError(f"Field item effects caller source inventory drift: {path}")
        total_calls += len(matches)
        callers[name] = {
            "callAddress": address,
            "returnAddress": address + 4,
            "instructionTarget": "UseItemOnField",
            "effectiveTarget": "UseItemOnField",
            "siteCount": len(matches),
        }
    if total_calls != 2:
        raise ValueError("Field item effects direct caller denominator drift")
    if complete_caller_paths is not None:
        expected_caller_paths = sorted(path for _, path, _ in call_sites)
        if complete_caller_paths != expected_caller_paths:
            raise ValueError("Field item effects complete direct caller inventory drift")

    usability_source = text["code/common/menus/item/isitemusableonfield.asm"]
    table_source = text["data/stats/items/usableoutsidebattleitems.asm"]
    table_items, table_sentinel = _parse_usable_item_table(table_source)
    _require_order(
        usability_source,
        (
            "IsItemUsableOnField:",
            "moveq #0,d2",
            "lea table_UsableOnFieldItems(pc),a0",
            "cmp.b (a0)+,d1",
            "beq.w @Return",
            "cmpi.b #-1,(a0)",
            "bne.s @Loop",
            "moveq #-1,d2",
        ),
        "usability function",
    )

    effects_source = text["code/common/menus/item/fielditemeffects.asm"]
    dispatch_order = _parse_dispatch_order(effects_source)
    _require_order(
        effects_source,
        (
            "UseItemOnField:",
            "movem.l d0-d1/d6-d7,-(sp)",
            "andi.w #ITEMENTRY_MASK_INDEX,d1",
            "movem.l d1/a0,-(sp)",
            "lea rjt_FieldItemEffects(pc),a0",
            "cmpi.w #-1,(a0)",
            "beq.w @Break",
            "cmp.w (a0)+,d1",
            "bne.w @Next",
            "move.w (a0)+,d1",
            "jsr rjt_FieldItemEffects(pc,d1.w)",
            "movem.l (sp)+,d1/a0",
            "movem.l (sp)+,d0-d1/d6-d7",
        ),
        "dispatch scan and register preservation",
    )

    effect_addresses = {
        "curePoison": _FUNCTION_ADDRESSES["fieldItem_CurePoison"],
        "curePoisonAndParalysis": _FUNCTION_ADDRESSES["fieldItem_CurePoisonAndParalysis"],
        "increaseAtt": _FUNCTION_ADDRESSES["fieldItem_IncreaseAtt"],
        "increaseDef": _FUNCTION_ADDRESSES["fieldItem_IncreaseDef"],
        "increaseAgi": _FUNCTION_ADDRESSES["fieldItem_IncreaseAgi"],
        "increaseMov": _FUNCTION_ADDRESSES["fieldItem_IncreaseMov"],
        "increaseHp": _FUNCTION_ADDRESSES["fieldItem_IncreaseHp"],
        "increaseMp": _FUNCTION_ADDRESSES["fieldItem_IncreaseMp"],
        "levelUp": _FUNCTION_ADDRESSES["fieldItem_LevelUp"],
    }
    cure_poison = _source_region(
        effects_source, "fieldItem_CurePoison:", "fieldItem_CurePoisonAndParalysis:", "CurePoison"
    )
    _require_order(
        cure_poison,
        (
            "jsr j_GetStatusEffects",
            "bclr #STATUSEFFECT_BIT_POISON,d1",
            "beq.s byte_22A64",
            "txt 149",
            "txt 148",
            "jsr j_SetStatusEffects",
        ),
        "CurePoison",
    )
    cure_both = _source_region(
        effects_source,
        "fieldItem_CurePoisonAndParalysis:",
        "fieldItem_IncreaseAtt:",
        "CurePoisonAndParalysis",
    )
    _require_order(
        cure_both,
        (
            "jsr j_GetStatusEffects",
            "moveq #0,d2",
            "bclr #STATUSEFFECT_BIT_POISON,d1",
            "beq.s loc_22A88",
            "moveq #-1,d2",
            "bclr #STATUSEFFECT_BIT_STUN,d1",
            "beq.s loc_22A98",
            "moveq #-1,d2",
            "tst.w d2",
            "bne.s loc_22AA0",
            "txt 148",
            "jsr j_SetStatusEffects",
            "jsr j_UpdateCombatantStats",
        ),
        "CurePoisonAndParalysis",
    )

    random_effects = {
        "increaseAtt": (
            "fieldItem_IncreaseAtt:",
            "fieldItem_IncreaseDef:",
            "IncreaseBaseAtt",
            "IncreaseCurrentAtt",
            150,
        ),
        "increaseDef": (
            "fieldItem_IncreaseDef:",
            "fieldItem_IncreaseAgi:",
            "IncreaseBaseDef",
            "IncreaseCurrentDef",
            151,
        ),
        "increaseAgi": (
            "fieldItem_IncreaseAgi:",
            "fieldItem_IncreaseMov:",
            "IncreaseBaseAgi",
            "IncreaseCurrentAgi",
            152,
        ),
        "increaseHp": (
            "fieldItem_IncreaseHp:",
            "fieldItem_IncreaseMp:",
            "IncreaseMaxHp",
            "IncreaseCurrentHp",
            154,
        ),
    }
    random_effect_text_ids: dict[str, list[int]] = {}
    for name, (start, end, base, current, text_id) in random_effects.items():
        region = _source_region(effects_source, start, end, name)
        _require_order(
            region,
            (
                "moveq #3,d6",
                "jsr (GenerateRandomNumber).w",
                "addq.w #2,d7",
                f"txt {text_id}",
                f"jsr j_{base}",
                "move.w d7,d1",
                f"jsr j_{current}",
            ),
            name,
        )
        random_effect_text_ids[name] = _exact_text_ids(effects_source, start, end, name)

    movement = _source_region(
        effects_source, "fieldItem_IncreaseMov:", "fieldItem_IncreaseHp:", "IncreaseMov"
    )
    _require_order(
        movement,
        (
            "jsr j_GetBaseMov",
            "clr.w d7",
            "cmpi.b #9,d1",
            "beq.w loc_22B42",
            "moveq #1,d7",
            "cmpi.b #8,d1",
            "beq.w loc_22B42",
            "moveq #2,d7",
            "jsr j_IncreaseBaseMov",
            "move.w d7,d1",
            "jsr j_IncreaseCurrentMov",
        ),
        "IncreaseMov",
    )
    movement_text_ids = _exact_text_ids(
        effects_source, "fieldItem_IncreaseMov:", "fieldItem_IncreaseHp:", "increaseMov"
    )
    mp = _source_region(effects_source, "fieldItem_IncreaseMp:", "fieldItem_LevelUp:", "IncreaseMp")
    _require_order(
        mp,
        (
            "jsr j_GetMaxMp",
            "tst.w d1",
            "beq.s byte_22BBC",
            "moveq #3,d6",
            "jsr (GenerateRandomNumber).w",
            "addq.w #2,d7",
            "txt 155",
            "jsr j_IncreaseMaxMp",
            "move.w d7,d1",
            "jsr j_IncreaseCurrentMp",
            "txt 148",
        ),
        "IncreaseMp",
    )
    mp_text_ids = sorted(
        _exact_text_ids(effects_source, "fieldItem_IncreaseMp:", "fieldItem_LevelUp:", "increaseMp")
    )
    level_up = _source_region(
        effects_source, "fieldItem_LevelUp:", "End of function fieldItem_LevelUp", "LevelUp"
    )
    _require_order(
        level_up,
        (
            "moveq #0,d1",
            "jsr j_SetCurrentExp",
            "jsr j_LevelUp",
            "lea ((LEVELUP_ARGUMENTS-$1000000)).w,a5",
            "cmpi.b #-1,d1",
            "bne.s loc_22BEA",
            "txt 148",
            "txt 244",
            "cmpi.b #-1,d1",
            "beq.w byte_22C5A",
            "lsr.w #SPELLENTRY_OFFSET_LV,d1",
            "bne.s loc_22C4C",
            "txt 271",
            "txt 272",
            "txt 3523",
        ),
        "LevelUp",
    )
    level_text_ids = _exact_text_ids(
        effects_source, "fieldItem_LevelUp:", "End of function fieldItem_LevelUp", "levelUp"
    )

    effects = {
        "curePoison": {
            "address": effect_addresses["curePoison"],
            "statusBit": constants["STATUSEFFECT_BIT_POISON"],
            "statusMask": constants["STATUSEFFECT_POISON"],
            "clearOperation": "bclr",
            "clearBranch": "beq",
            "callOrder": ["GetStatusEffects", "SetStatusEffects"],
            "getStatusEffectsAddress": _FUNCTION_ADDRESSES["GetStatusEffects"],
            "textIds": _exact_text_ids(
                effects_source,
                "fieldItem_CurePoison:",
                "fieldItem_CurePoisonAndParalysis:",
                "curePoison",
            ),
        },
        "curePoisonAndParalysis": {
            "address": effect_addresses["curePoisonAndParalysis"],
            "statusBits": [
                constants["STATUSEFFECT_BIT_POISON"],
                constants["STATUSEFFECT_BIT_STUN"],
            ],
            "statusMasks": [constants["STATUSEFFECT_POISON"], constants["STATUSEFFECT_STUN"]],
            "clearOperation": "bclr",
            "effectPresentBranch": "bne",
            "callOrder": ["GetStatusEffects", "SetStatusEffects", "UpdateCombatantStats"],
            "updateCombatantStatsAddress": _FUNCTION_ADDRESSES["UpdateCombatantStats"],
            "textIds": _exact_text_ids(
                effects_source,
                "fieldItem_CurePoisonAndParalysis:",
                "fieldItem_IncreaseAtt:",
                "curePoisonAndParalysis",
            ),
        },
        **{
            name: {
                "address": effect_addresses[name],
                "textIds": random_effect_text_ids[name],
                "baseTarget": base,
                "currentTarget": current,
                "callOrder": ["GenerateRandomNumber", base, current],
            }
            for name, (_, _, base, current, text_id) in random_effects.items()
        },
        "increaseMov": {
            "address": effect_addresses["increaseMov"],
            "baseTarget": "GetBaseMov",
            "incrementsByBase": [
                {"base": 9, "gain": 0},
                {"base": 8, "gain": 1},
                {"base": "default", "gain": 2},
            ],
            "callOrder": ["GetBaseMov", "IncreaseBaseMov", "IncreaseCurrentMov"],
            "textIds": movement_text_ids,
        },
        "increaseMp": {
            "address": effect_addresses["increaseMp"],
            "maxMpTarget": "GetMaxMp",
            "zeroMaxMpBranch": "beq",
            "noUseTextId": 148,
            "callOrder": ["GetMaxMp", "GenerateRandomNumber", "IncreaseMaxMp", "IncreaseCurrentMp"],
            "textIds": mp_text_ids,
        },
        "levelUp": {
            "address": effect_addresses["levelUp"],
            "setCurrentExpValue": 0,
            "levelUpAddress": _FUNCTION_ADDRESSES["LevelUp"],
            "argumentsAddress": _FUNCTION_ADDRESSES["LEVELUP_ARGUMENTS"],
            "levelResultSentinel": -1,
            "levelResultBranch": "bne",
            "zeroStatGainBranch": "beq",
            "spellSentinel": -1,
            "spellSentinelBranch": "beq",
            "spellLevelBranch": "bne",
            "textIds": level_text_ids,
        },
        "dispatchOrder": dispatch_order,
    }
    public_text_ids = sorted(
        {
            value
            for effect in effects.values()
            if isinstance(effect, dict)
            for value in effect.get("textIds", [])
        }
    )
    if public_text_ids != [
        148,
        149,
        150,
        151,
        152,
        153,
        154,
        155,
        156,
        244,
        266,
        267,
        268,
        269,
        270,
        271,
        272,
        3523,
    ]:
        raise ValueError("Field item effects public text-ID domain drift")

    return {
        "sourceContext": {
            "layoutCanonicalIncludes": list(canonical_includes),
            "excludedAlternates": ["code/common/stats/items/fielditemeffects.asm"],
            "callerInventory": {
                "instructionTarget": "UseItemOnField",
                "effectiveTarget": "UseItemOnField",
                "instructionTargetSiteCount": total_calls,
                "effectiveTargetSiteCount": total_calls,
            },
        },
        "fieldItemEffects": {
            "callers": callers,
            "usability": {
                "functionAddress": _FUNCTION_ADDRESSES["IsItemUsableOnField"],
                "tableAddress": _FUNCTION_ADDRESSES["table_UsableOnFieldItems"],
                "itemIds": table_items,
                "sentinel": table_sentinel,
                "inputRegister": "d1",
                "resultRegister": "d2",
                "matchResult": 0,
                "notFoundResult": -1,
                "preservedRegisters": ["d1"],
            },
            "dispatch": {
                "functionAddress": _FUNCTION_ADDRESSES["UseItemOnField"],
                "tableAddress": _FUNCTION_ADDRESSES["rjt_FieldItemEffects"],
                "itemEntryMask": constants["ITEMENTRY_MASK_INDEX"],
                "sentinel": -1,
                "notFoundBranch": "beq",
                "preservedRegisters": ["d0", "d1", "d6", "d7", "a0"],
            },
            "randomGain": {
                "generatorAddress": _FUNCTION_ADDRESSES["GenerateRandomNumber"],
                "boundRegister": "d6",
                "bound": 3,
                "resultRegister": "d7",
                "postIncrement": 2,
                "gainRange": [2, 4],
            },
            "effects": effects,
        },
    }


def _structural_schema() -> dict[str, Any]:
    schema = load_json(SCHEMA)
    fixture = schema.get("$defs", {}).get("fixture")
    if not isinstance(fixture, dict):
        raise ValueError("Field item effects fixture schema definition is missing")
    return {"$schema": schema["$schema"], "$ref": "#/$defs/fixture", "$defs": schema["$defs"]}


def _validate_structural_output(value: dict[str, Any]) -> None:
    errors = sorted(
        Draft7Validator(_structural_schema()).iter_errors(value),
        key=lambda error: list(error.absolute_path),
    )
    if errors:
        location = ".".join(str(part) for part in errors[0].absolute_path) or "<root>"
        message = errors[0].message
        raise ValueError(
            f"Field item effects structural schema validation failed at {location}: {message}"
        )


def build_field_item_effects_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the deterministic public H2 field-item-effects contract; no H3 runs."""
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != _ROM_SHA256:
        raise ValueError("Field item effects canonical ROM SHA-256 drift")
    upstream = upstream_path.resolve(strict=True)
    revision = subprocess.run(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if revision != _UPSTREAM_COMMIT:
        raise ValueError("Field item effects upstream revision drift")
    source, identities = _read_source_surface(_disasm_root(upstream))
    h1_binary = (upstream / _H1_BINARY).read_bytes()
    rom = rom_path.resolve(strict=True).read_bytes()
    addresses = listing_symbol_addresses((upstream / _LISTING).read_text(encoding="utf-8"))
    expected_symbols = {
        "IsItemUsableOnField": _FUNCTION_ADDRESSES["IsItemUsableOnField"],
        "table_UsableOnFieldItems": _FUNCTION_ADDRESSES["table_UsableOnFieldItems"],
        "UseItemOnField": _FUNCTION_ADDRESSES["UseItemOnField"],
        "rjt_FieldItemEffects": _FUNCTION_ADDRESSES["rjt_FieldItemEffects"],
        "fieldItem_CurePoison": _FUNCTION_ADDRESSES["fieldItem_CurePoison"],
        "fieldItem_CurePoisonAndParalysis": _FUNCTION_ADDRESSES["fieldItem_CurePoisonAndParalysis"],
        "fieldItem_IncreaseAtt": _FUNCTION_ADDRESSES["fieldItem_IncreaseAtt"],
        "fieldItem_IncreaseDef": _FUNCTION_ADDRESSES["fieldItem_IncreaseDef"],
        "fieldItem_IncreaseAgi": _FUNCTION_ADDRESSES["fieldItem_IncreaseAgi"],
        "fieldItem_IncreaseMov": _FUNCTION_ADDRESSES["fieldItem_IncreaseMov"],
        "fieldItem_IncreaseHp": _FUNCTION_ADDRESSES["fieldItem_IncreaseHp"],
        "fieldItem_IncreaseMp": _FUNCTION_ADDRESSES["fieldItem_IncreaseMp"],
        "fieldItem_LevelUp": _FUNCTION_ADDRESSES["fieldItem_LevelUp"],
    }
    if {name: addresses.get(name) for name in expected_symbols} != expected_symbols:
        raise ValueError("Field item effects H1 symbol projection drift")
    parsed = _validate_source_contract(
        source, complete_caller_paths=_complete_code_caller_paths(_disasm_root(upstream))
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
        "sourceContext": {
            "sourceIdentities": identities,
            "h1RomAnchors": _anchor_projection(h1_binary, rom),
            **parsed["sourceContext"],
        },
        "retainedOwners": _retained_owners(),
        "fieldItemEffects": parsed["fieldItemEffects"],
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": {"sourceFiles": 8, "h1RomAnchors": 14, "callers": 2, "unknowns": 12},
    }
    _validate_structural_output(output)
    return output


def verify_field_item_effects_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Validate the checked-in fixture against fresh source/H1/ROM derivation."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    output = build_field_item_effects_static(rom_path, upstream_path)
    if fixture != output:
        raise ValueError("Field item effects complete semantic fixture drift")
    return output
