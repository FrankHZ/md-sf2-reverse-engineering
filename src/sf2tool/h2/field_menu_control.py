"""Public H2 source/H1/ROM contract for the FieldMenu control surface."""

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

ID = "sf2-field-menu-control-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/field-menu-control-static-v1.json")
SCHEMA = repo_path("schemas/h2/field-menu-control-static-fixture.schema.json")
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
    "code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm",
    "code/common/tech/jumpinterfaces/s05_jumpinterface.asm",
    "code/common/menus/main/mainactions.asm",
    "code/gameflow/exploration/explorationvints.asm",
    "code/gameflow/special/battletest.asm",
    "code/gameflow/exploration/explorationfunctions_0.asm",
)

_SOURCE_SHA256 = dict(
    zip(
        _SOURCE_SURFACE,
        (
            "17738776F811F66322F278CFBF10E8B376612F4F9EDFC2C7AA0A5DB81CDFB3FB",
            "ABA0DFEE4F4D3BFCD49646C03B5229DD6458FFC3A90174907823D710884020EE",
            "F5FD087710CE328C5D3DD8ADBCE2CDFD8D60F4F850743BA020F69EC56E5D63C6",
            "05519524B04D8E99050600CCAD13E6CC3465417737B2AB21C7330F72C8F3F5C6",
            "175361E5E65A358B02705C0A4C61D55500209429F88D2C1FAEF4F4113E6975CE",
            "E69B3D40DF6658CF761A035FA1B6643DD9770B3C82F4CBB21D5AC4D3A50F2777",
            "977A8D25FC1F8B155A5AC8D7360A6513C0C7A76ADCCC5C6A29EB55D34DB66D3B",
            "4346CC876F8FE204463559EF0C523E9A2B37C238FC4554853498282A2E47BB98",
            "BBFED0DF86D68A813047650B3E10754382881C17B2F644F8F1BC897F77EA3B4E",
        ),
        strict=True,
    )
)

_FUNCTION_ADDRESSES = {
    "j_FieldMenu": 0x20008,
    "FieldMenu": 0x2127E,
    "PopulateGenericListWithCurrentForceMembers": 0x219EC,
    "j_ExecuteDiamondMenu": 0x10000,
    "ExecuteDiamondMenu": 66038,
    "j_ExecuteMembersListScreenOnMainSummaryPage": 0x10040,
    "ExecuteMembersListScreenOnMainSummaryPage": 77798,
    "j_ExecuteMembersListScreenOnItemSummaryPage": 0x10044,
    "ExecuteMembersListScreenOnItemSummaryPage": 77828,
    "j_ExecuteMembersListScreenOnMagicSummaryPage": 0x10048,
    "ExecuteMembersListScreenOnMagicSummaryPage": 77872,
    "BuildMemberScreen": 72746,
    "j_alt_YesNoPrompt": 0x10074,
    "alt_YesNoPrompt": 86668,
    "IsItemUsableOnField": 141770,
    "UseItemOnField": 141804,
    "RunMapSetupItemEvent": 292230,
    "GetSavepointForMap": 30188,
    "CheckArea": 145506,
    "UpdateForce": 39168,
}

_ANCHORS = (
    ("entryAndCallers.debugCall", 0x7884, 6, None),
    ("jumpInterfaces.ExecuteDiamondMenu", 0x10000, 4, None),
    ("jumpInterfaces.MembersMain", 0x10040, 4, None),
    ("jumpInterfaces.MembersItem", 0x10044, 4, None),
    ("jumpInterfaces.MembersMagic", 0x10048, 4, None),
    ("entryAndCallers.jumpInterface", 0x20008, 4, None),
    ("functionAddresses.FieldMenu", 0x2127E, 1902, 0x219EC),
    ("functionAddresses.PopulateGenericListWithCurrentForceMembers", 0x219EC, 42, 0x21A16),
    ("excludedUnusedTail.table", 0x21A16, 6, 0x21A1C),
    ("excludedUnusedTail.helper", 0x21A1C, 30, 0x21A3A),
    ("searchAction.CheckArea", 0x23862, 2, None),
    ("entryAndCallers.explorationCall", 0x25BDC, 6, None),
    ("effectiveTargets.ExecuteDiamondMenu", 66038, 2, None),
    ("effectiveTargets.MembersMain", 77798, 2, None),
    ("effectiveTargets.MembersItem", 77828, 2, None),
    ("effectiveTargets.MembersMagic", 77872, 2, None),
    ("memberAction.BuildMemberScreen", 72746, 2, None),
    ("itemAction.AltYesNoPrompt", 86668, 2, None),
    ("itemAction.IsItemUsableOnField", 141770, 2, None),
    ("itemAction.UseItemOnField", 141804, 2, None),
    ("itemAction.RunMapSetupItemEvent", 292230, 2, None),
    ("magicAction.GetSavepointForMap", 30188, 2, None),
    ("forceListHelper.UpdateForce", 39168, 2, None),
)

_FIELD_MENU_SPAN_SHA256 = "F160CD3803063AE4E2FAD59389803B2083EE0811CA387258957F68EB11AB69ED"
_UNKNOWN_KEYS = (
    "natural-story-field-menu-reachability",
    "caller-entry-state",
    "actual-main-choice",
    "actual-member-selection-and-return",
    "actual-magic-selection-and-target",
    "actual-detox-status-outcome",
    "actual-egress-map-and-event-outcome",
    "actual-item-submenu-choice",
    "actual-item-use-selection-target-and-result",
    "actual-map-item-event-consumption",
    "actual-item-give-or-exchange",
    "actual-item-equip-result",
    "actual-item-drop-confirmation-and-result",
    "actual-search-area-result",
    "input-repeat-cancel-and-cadence",
    "window-cursor-portrait-text-sound-presentation",
    "persistence-across-map-save-story",
    "caller-return-state-and-vint-reactivation",
)

_RETAINED_OWNER_PATHS = {
    "commonMenus": "tests/fixtures/h2/common-menus-static-v1.json",
    "gameflowCore": "tests/fixtures/h2/gameflow-core-static-v1.json",
    "coreStatsData": "tests/fixtures/h2/core-stats-data-static-v1.json",
    "itemAuxiliary": "tests/fixtures/h2/item-auxiliary-static-v1.json",
    "mapSetup": "tests/fixtures/h2/map-setup-static-v1.json",
    "techInterfaces": "tests/fixtures/h2/tech-interfaces-static-v1.json",
    "commonStats": "tests/fixtures/h2/common-stats-static-v1.json",
}


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Emit the sole canonical UTF-8 representation for this public fixture."""
    return json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8") + bytes((10,))


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
            raise ValueError(f"FieldMenu {context} source-use drift: {fragment}")
        cursor = found + len(fragment)


def _source_region(source: str, start: str, end: str, context: str) -> str:
    """Return one bounded source use-site, excluding its closing marker."""
    start_at = source.find(start)
    if start_at < 0:
        raise ValueError(f"FieldMenu {context} source-use start drift: {start}")
    end_at = source.find(end, start_at + len(start))
    if end_at < 0:
        raise ValueError(f"FieldMenu {context} source-use end drift: {end}")
    return source[start_at:end_at]


def _ordered_distinct(values: list[int]) -> list[int]:
    return list(dict.fromkeys(values))


def _text_ids_at_use_site(source: str, start: str, end: str, context: str) -> list[int]:
    region = _source_region(source, start, end, context)
    ids = _ordered_distinct(
        [
            int(value)
            for value in re.findall(r"^\s*txt\s+(\d+)\b", _without_comments(region), re.MULTILINE)
        ]
    )
    if not ids:
        raise ValueError(f"FieldMenu {context} text-ID use-site drift")
    return ids


def _call_order_at_use_site(source: str, start: str, end: str, context: str) -> list[str]:
    region = _source_region(source, start, end, context)
    calls = [target.removeprefix("j_") for target in _direct_calls(region)]
    if not calls:
        raise ValueError(f"FieldMenu {context} call-order use-site drift")
    return calls


def _read_source_surface(root: Path) -> tuple[dict[str, str], list[dict[str, str]]]:
    text: dict[str, str] = {}
    identities: list[dict[str, str]] = []
    for relative in _SOURCE_SURFACE:
        path = root / relative
        if not path.is_file():
            raise ValueError(f"FieldMenu source is missing: {relative}")
        data = path.read_bytes()
        sha256 = hashlib.sha256(data).hexdigest().upper()
        if sha256 != _SOURCE_SHA256[relative]:
            raise ValueError(f"FieldMenu source hash drift: {relative}")
        identities.append({"path": relative, "sha256": sha256})
        text[relative] = data.decode("utf-8").replace("\r\n", "\n")
    if len(identities) != 9:
        raise ValueError("FieldMenu source denominator drift")
    return text, identities


def _source_forms(source: str) -> list[str]:
    forms: list[str] = []
    for raw in _without_comments(source).splitlines():
        clean = raw.strip()
        if not clean:
            continue
        form = re.sub(r"^(?:@?[A-Za-z_][A-Za-z0-9_]*):\s*", "", clean)
        if form:
            forms.append(form)
    return forms


def _direct_calls(source: str) -> list[str]:
    targets: list[str] = []
    for raw in _without_comments(source).splitlines():
        match = re.match(
            r"^\s*(?:jsr|bsr)(?:\.\w+)?\s+(?:\(([^)]+)\)\.\w+|([A-Za-z_@][A-Za-z0-9_@]*))\s*$",
            raw,
        )
        if match is None:
            continue
        target = match.group(1) or match.group(2)
        targets.append(target)
    return targets


def _parse_equ(source: str) -> dict[str, int]:
    values: dict[str, int] = {}
    for raw in _without_comments(source).splitlines():
        match = re.match(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*:?\s+equ\s+([^\s]+)", raw)
        if match is None:
            match = re.match(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\s+equ\s+([^\s]+)", raw)
        if match is None:
            continue
        literal = match.group(2)
        if literal.startswith("$"):
            values[match.group(1)] = int(literal[1:], 16)
        elif literal.lstrip("-").isdigit():
            values[match.group(1)] = int(literal)
    return values


def _anchor_projection(h1_binary: bytes, rom: bytes) -> list[dict[str, Any]]:
    anchors: list[dict[str, Any]] = []
    for identifier, address, width, end_address in _ANCHORS:
        h1 = h1_binary[address : address + width]
        rom_bytes = rom[address : address + width]
        if len(h1) != width or len(rom_bytes) != width or h1 != rom_bytes:
            raise ValueError(f"FieldMenu H1/ROM anchor drift: {identifier}")
        item: dict[str, Any] = {
            "id": identifier,
            "address": address,
            "width": width,
            "sha256": hashlib.sha256(h1).hexdigest().upper(),
        }
        if end_address is not None:
            item["endAddressExclusive"] = end_address
        anchors.append(item)
    field_menu = next(anchor for anchor in anchors if anchor["id"] == "functionAddresses.FieldMenu")
    if field_menu["sha256"] != _FIELD_MENU_SPAN_SHA256:
        raise ValueError("FieldMenu full span SHA-256 drift")
    if len(anchors) != 23:
        raise ValueError("FieldMenu H1/ROM anchor denominator drift")
    return anchors


def _fresh_guard_retained_owners(rom_path: Path, upstream_path: Path) -> None:
    """Consume the seven accepted owners without copying their algorithms or goldens."""
    from sf2tool.h2.core_stats_data import build_core_stats_data_inventory
    from sf2tool.h2.gameflow import build_gameflow_inventory
    from sf2tool.h2.interfaces import build_interface_inventory
    from sf2tool.h2.item_auxiliary import build_item_auxiliary_contract
    from sf2tool.h2.map_setup import build_map_setup_contract
    from sf2tool.h2.menus import _verify_menu_fixture_owner, build_menu_inventory
    from sf2tool.h2.stats import _verify_stats_fixture_owner, build_stats_inventory

    fixtures = {
        name: load_json(repo_path(relative)) for name, relative in _RETAINED_OWNER_PATHS.items()
    }
    rom_manifest = load_json(ROM_MANIFEST)
    research_index = load_json(repo_path("manifests/research-index.json"))

    menus = build_menu_inventory(upstream_path)
    _verify_menu_fixture_owner(fixtures["commonMenus"], menus, rom_manifest=rom_manifest)

    gameflow = build_gameflow_inventory(upstream_path)
    gameflow_fixture = fixtures["gameflowCore"]
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "startupFacts",
        "explorationFacts",
        "runtimeQuestions",
    ):
        if gameflow[field] != gameflow_fixture["expected"][field]:
            raise ValueError(f"FieldMenu retained gameflowCore join drift: {field}")
    if gameflow["representativeAddresses"] != gameflow_fixture["function"]:
        raise ValueError("FieldMenu retained gameflowCore address join drift")

    core_stats_data = build_core_stats_data_inventory(upstream_path)
    core_stats_fixture = fixtures["coreStatsData"]
    if core_stats_data["representativeAddresses"] != core_stats_fixture["table"]:
        raise ValueError("FieldMenu retained coreStatsData address join drift")
    for field in ("facts", "runtimeQuestions"):
        if core_stats_data[field] != core_stats_fixture["expected"][field]:
            raise ValueError(f"FieldMenu retained coreStatsData join drift: {field}")

    item_auxiliary = build_item_auxiliary_contract(rom_path, upstream_path)
    item_fixture = fixtures["itemAuxiliary"]
    item_table = {row["symbol"]: row["address"] for row in item_auxiliary["romRanges"]}
    for field, actual in (
        ("table", item_table),
        ("summary", item_auxiliary["summary"]),
        ("consumerRules", item_auxiliary["consumerRules"]),
        ("runtimeQuestions", item_auxiliary["runtimeQuestions"]),
    ):
        if item_fixture[field] != actual:
            raise ValueError(f"FieldMenu retained itemAuxiliary join drift: {field}")

    map_setup = build_map_setup_contract(rom_path, upstream_path)
    map_fixture = fixtures["mapSetup"]
    if (
        map_setup["function"] != map_fixture["function"]
        or map_setup["table"] != map_fixture["table"]
    ):
        raise ValueError("FieldMenu retained mapSetup address join drift")
    for field in (
        "summary",
        "sourceFacts",
        "aliasFlagRoutes",
        "selectionCases",
        "runtimeQuestions",
    ):
        if map_setup[field] != map_fixture["expected"][field]:
            raise ValueError(f"FieldMenu retained mapSetup join drift: {field}")

    interfaces = build_interface_inventory(upstream_path)
    interfaces_fixture = fixtures["techInterfaces"]
    if interfaces["representativeAddresses"] != interfaces_fixture["function"]:
        raise ValueError("FieldMenu retained techInterfaces address join drift")
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
        "interfaceFacts",
    ):
        if interfaces[field] != interfaces_fixture["expected"][field]:
            raise ValueError(f"FieldMenu retained techInterfaces join drift: {field}")

    stats = build_stats_inventory(upstream_path)
    _verify_stats_fixture_owner(
        fixtures["commonStats"],
        stats,
        rom_manifest=rom_manifest,
        research_index=research_index,
    )


def _retained_usable_field_items_address() -> int:
    table = load_json(repo_path(_RETAINED_OWNER_PATHS["coreStatsData"]))["table"]
    address = table.get("table_UsableOnFieldItems")
    if not isinstance(address, int) or address < 0:
        raise ValueError("FieldMenu retained usable-field-items address drift")
    return address


def _retained_owners(rom_path: Path, upstream_path: Path) -> dict[str, dict[str, str]]:
    _fresh_guard_retained_owners(rom_path, upstream_path)
    owners: dict[str, dict[str, str]] = {}
    for name, relative in _RETAINED_OWNER_PATHS.items():
        path = repo_path(relative)
        fixture = load_json(path)
        fixture_id = fixture.get("id")
        if not isinstance(fixture_id, str):
            raise ValueError(f"FieldMenu retained owner fixture identity drift: {relative}")
        owners[name] = {
            "fixtureId": fixture_id,
            "fixtureSha256": hashlib.sha256(path.read_bytes()).hexdigest().upper(),
        }
    if len(owners) != 7:
        raise ValueError("FieldMenu retained-owner denominator drift")
    return owners


def _validate_source_contract(
    text: dict[str, str], *, usable_field_items_address: int
) -> dict[str, Any]:
    """Parse the bounded FieldMenu control graph without claiming runtime behavior."""
    main = text["code/common/menus/main/mainactions.asm"]
    enums = _parse_equ(text["sf2enums.asm"])
    forms = _source_forms(main)
    globals_ = re.findall(r"^\s*([A-Za-z_][A-Za-z0-9_]*):", _without_comments(main), re.MULTILINE)
    locals_ = re.findall(r"^\s*(@[A-Za-z_][A-Za-z0-9_]*):", _without_comments(main), re.MULTILINE)
    calls = _direct_calls(main)
    physical_lines = len(main.splitlines())
    non_empty_lines = sum(line != "" for line in main.splitlines())
    if physical_lines != 696 or non_empty_lines != 679 or len(forms) != 481:
        raise ValueError("FieldMenu mainactions line or statement denominator drift")
    if len(globals_) != 12 or len(locals_) != 46:
        raise ValueError("FieldMenu mainactions label denominator drift")
    if len(calls) != 69 or len(set(calls)) != 33:
        raise ValueError("FieldMenu mainactions direct-call denominator drift")

    expected_constants = {
        "MENU_FIELD": 0,
        "MENU_ITEM": 3,
        "ITEM_SUBMENU_ACTION_USE": 0,
        "ITEM_SUBMENU_ACTION_GIVE": 1,
        "ITEM_SUBMENU_ACTION_DROP": 2,
        "ITEM_SUBMENU_ACTION_EQUIP": 3,
        "SPELL_DETOX": 2,
        "SPELL_EGRESS": 10,
        "ITEM_ANGEL_WING": 4,
        "MAP_OVERWORLD_GRANSEAL_KINGDOM": 66,
        "MAP_OVERWORLD_PACALON_KINGDOM": 78,
        "ITEMTYPE_RARE": 8,
        "ITEMTYPE_UNSELLABLE": 16,
        "EQUIPMENTTYPE_TOOL": 0,
        "EQUIPMENTTYPE_WEAPON": 1,
        "COMBATANT_ITEMSLOTS": 4,
    }
    if {key: enums.get(key) for key in expected_constants} != expected_constants:
        raise ValueError("FieldMenu authoritative constants drift")

    egress_text_ids = _text_ids_at_use_site(main, "@StartMagic:", "byte_213A8:", "Egress")
    detox_text_ids = _text_ids_at_use_site(main, "byte_213A8:", "@ExitMagic:", "Detox")
    item_use_text_ids = _text_ids_at_use_site(
        main, "@StartItemUse:", "@IsItemGiveAction:", "item use"
    )
    item_give_text_ids = _text_ids_at_use_site(
        main, "@StartItemGive:", "@IsItemEquipAction:", "item give"
    )
    item_drop_text_ids = _text_ids_at_use_site(main, "@ItemDropAction:", "byte_219D0:", "item drop")
    if (
        egress_text_ids != [243, 312]
        or detox_text_ids != [108, 243, 303, 302, 301, 422]
        or item_use_text_ids != [73, 422]
        or item_give_text_ids != [55, 54, 74, 65, 66]
        or item_drop_text_ids != [62, 69, 55, 67]
    ):
        raise ValueError("FieldMenu text-ID use-site projection drift")

    exchange_call_order = _call_order_at_use_site(
        main, "@StartExchange:", "@SkipExchange:", "item exchange"
    )
    if exchange_call_order != [
        "UnequipItemBySlotIfNotCursed",
        "DropItemBySlot",
        "AddItem",
        "RemoveItemBySlot",
        "AddItem",
    ]:
        raise ValueError("FieldMenu item exchange call-order projection drift")

    _require_order(
        main,
        (
            "FieldMenu:",
            "@StartMain:",
            "moveq #0,d0",
            "moveq #0,d1",
            "moveq #MENU_FIELD,d2",
            "jsr j_ExecuteDiamondMenu",
            "cmpi.w #-1,d0",
            "beq.s @ExitMain",
            "@IsMemberAction:",
            "cmpi.w #0,d0",
            "bne.w @IsMagicAction",
            "@IsMagicAction:",
            "cmpi.w #1,d0",
            "bne.w @IsItemAction",
            "@IsItemAction:",
            "cmpi.w #2,d0",
            "bne.w @SearchAction",
        ),
        "main selector",
    )
    _require_order(
        main,
        (
            "@StartMember:",
            "move.b #ITEM_SUBMENU_ACTION_USE,((CURRENT_ITEM_SUBMENU_ACTION-$1000000)).w",
            "jsr j_ExecuteMembersListScreenOnMainSummaryPage",
            "cmpi.w #-1,d0",
            "beq.w @ExitMember",
            "jsr j_BuildMemberScreen",
            "bra.s @StartMember",
        ),
        "member handoff and return",
    )
    _require_order(
        main,
        (
            "@StartMagic:",
            "jsr j_ExecuteMembersListScreenOnMagicSummaryPage",
            "cmpi.w #-1,d0",
            "beq.w @ExitMagic",
            "andi.w #SPELLENTRY_MASK_INDEX,spellIndex(a6)",
            "lsr.l #SPELLENTRY_OFFSET_LV,d1",
            "addq.l #1,d1",
            "cmpi.w #SPELL_DETOX,spellIndex(a6)",
            "beq.w byte_213A8",
            "cmpi.w #SPELL_EGRESS,spellIndex(a6)",
            "beq.w @IsOnOverworldMap",
            "@IsOnOverworldMap:",
            "cmpi.w #MAP_OVERWORLD_GRANSEAL_KINGDOM,d0",
            "blt.s byte_21348",
            "cmpi.w #MAP_OVERWORLD_PACALON_KINGDOM,d0",
            "bgt.s byte_21348",
            "@Egress:",
            "jsr j_GetSpellDefinitionAddress",
            "jsr j_DecreaseCurrentMp",
            "jsr j_ExecuteFlashScreenScript",
            "jsr (GetSavepointForMap).w",
            "move.w #1,(a0)+",
            "move.b #0,(a0)+",
            "move.b d0,(a0)+",
            "move.b d1,(a0)+",
            "move.b d2,(a0)+",
            "move.b d3,(a0)+",
            "clr.b ((PLAYER_TYPE-$1000000)).w",
        ),
        "magic selection and Egress order",
    )
    _require_order(
        main,
        (
            "byte_213A8:",
            "jsr j_ExecuteMembersListScreenOnItemSummaryPage",
            "cmpi.w #-1,d0",
            "beq.w @StartMagic",
            "jsr j_GetSpellDefinitionAddress",
            "jsr j_DecreaseCurrentMp",
            "jsr j_GetStatusEffects",
            "cmpi.l #1,spellLevel(a6)",
            "beq.w @CurePoison",
            "cmpi.l #2,spellLevel(a6)",
            "beq.w @CureStun",
            "jsr j_UnequipAllItemsIfNotCursed",
            "jsr j_SetStatusEffects",
            "jsr j_UpdateCombatantStats",
        ),
        "Detox order",
    )
    _require_order(
        main,
        (
            "@StartItemSubmenu:",
            "moveq #MENU_ITEM,d2",
            "jsr j_ExecuteDiamondMenu",
            "cmpi.w #-1,d0",
            "beq.w @StartMain",
            "cmpi.w #0,d0",
            "bne.w @IsItemGiveAction",
            "@IsItemGiveAction:",
            "cmpi.w #1,d0",
            "bne.w @IsItemEquipAction",
            "@IsItemEquipAction:",
            "cmpi.w #2,d0",
            "bne.w @ItemDropAction",
        ),
        "item submenu selector",
    )
    _require_order(
        main,
        (
            "@StartItemUse:",
            "cmpi.w #ITEM_ANGEL_WING,d2",
            "bne.w @HandleNonAngelWingItems",
            "cmpi.w #MAP_OVERWORLD_GRANSEAL_KINGDOM,d0",
            "blt.w @HandleNonAngelWingItems",
            "cmpi.w #MAP_OVERWORLD_PACALON_KINGDOM,d0",
            "bgt.w @HandleNonAngelWingItems",
            "jsr j_RemoveItemBySlot",
            "bra.w @Egress",
            "@HandleNonAngelWingItems:",
            "jsr IsItemUsableOnField",
            "tst.w d2",
            "beq.w @PickTarget",
            "bsr.w GetPlayerEntityPosition",
            "jsr j_RunMapSetupItemEvent",
            "tst.w d6",
            "bne.w @ExitMain",
            "@PickTarget:",
            "move.b #ITEM_SUBMENU_ACTION_USE,((CURRENT_ITEM_SUBMENU_ACTION-$1000000)).w",
            "jsr j_ExecuteMembersListScreenOnMainSummaryPage",
            "cmpi.w #-1,d0",
            "beq.w @StartItemUse",
            "bsr.w UseItemOnField",
            "jsr j_RemoveItemBySlot",
        ),
        "item use branches",
    )
    _require_order(
        main,
        (
            "@StartItemGive:",
            "jsr j_ExecuteMembersListScreenOnItemSummaryPage",
            "cmpi.w #-1,d0",
            "bne.w @IsGivingWeapon",
            "@IsGivingWeapon:",
            "jsr j_GetEquipmentType",
            "cmpi.w #EQUIPMENTTYPE_WEAPON,d2",
            "bne.s @IsGivingUnequippableItem",
            "jsr j_GetEquippedWeapon",
            "jsr j_IsItemCursed",
            "@IsGivingUnequippableItem:",
            "cmpi.w #EQUIPMENTTYPE_TOOL,d2",
            "beq.w @PickRecipient",
            "jsr j_GetEquippedRing",
            "@PickRecipient:",
            "jsr j_ExecuteMembersListScreenOnItemSummaryPage",
            "cmpi.w #-1,d0",
            "bne.w @GiveItem",
            "@GiveItem:",
            "jsr j_GetItemBySlotAndHeldItemsNumber",
            "cmpi.w #COMBATANT_ITEMSLOTS,d2",
            "beq.w @ExchangeItems",
            "jsr j_RemoveItemBySlot",
            "jsr j_AddItem",
            "@ExchangeItems:",
            "jsr j_UnequipItemBySlotIfNotCursed",
            "jsr j_DropItemBySlot",
        ),
        "item give and exchange branches",
    )
    _require_order(
        _source_region(main, "@IsItemEquipAction:", "@ItemDropAction:", "item equip"),
        (
            "@IsItemEquipAction:",
            "cmpi.w #2,d0",
            "bne.w @ItemDropAction",
            "bsr.w PopulateGenericListWithCurrentForceMembers",
            "move.b #ITEM_SUBMENU_ACTION_EQUIP,((CURRENT_ITEM_SUBMENU_ACTION-$1000000)).w",
            "move.w #ITEM_NOTHING,((SELECTED_ITEM_INDEX-$1000000)).w",
            "jsr j_ExecuteMembersListScreenOnItemSummaryPage",
            "cmpi.w #-1,d0",
            "beq.w @Goto_ExitItemEquip",
            "bra.w @ExitItemEquip",
            "@Goto_ExitItemEquip:",
            "bra.w @ExitItemEquip",
            "@ExitItemEquip:",
            "bra.w @Goto_StartItemSubmenu",
        ),
        "item equip handoff",
    )
    _require_order(
        main,
        (
            "@ItemDropAction:",
            "jsr j_GetItemDefinitionAddress",
            "andi.b #ITEMTYPE_UNSELLABLE,d1",
            "cmpi.b #0,d1",
            "beq.s @ConfirmDrop",
            "@ConfirmDrop:",
            "jsr j_alt_YesNoPrompt",
            "cmpi.w #0,d0",
            "beq.w @IsDroppingWeapon",
            "@IsDroppingWeapon:",
            "jsr j_GetEquipmentType",
            "jsr j_GetEquippedWeapon",
            "jsr j_IsItemCursed",
            "@IsDroppingUnequippableItem:",
            "jsr j_GetEquippedRing",
            "@DropItem:",
            "jsr j_RemoveItemBySlot",
            "andi.b #ITEMTYPE_RARE,d1",
            "cmpi.b #0,d1",
            "beq.s byte_219D0",
            "jsr j_AddItemToDeals",
        ),
        "item drop branches",
    )
    _require_order(
        main,
        (
            "@SearchAction:",
            "clr.w d6",
            "jsr j_CheckArea",
            "bra.w @ExitMain",
            "PopulateGenericListWithCurrentForceMembers:",
            "jsr j_UpdateForce",
            "move.w ((TARGETS_LIST_LENGTH-$1000000)).w,((GENERIC_LIST_LENGTH-$1000000)).w",
            "move.w ((TARGETS_LIST_LENGTH-$1000000)).w,d7",
            "subq.w #1,d7",
            "move.b (a0)+,(a1)+",
            "dbf d7,@Copy_Loop",
        ),
        "search and force-list helper",
    )

    aliases = text["code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm"]
    aliases_s05 = text["code/common/tech/jumpinterfaces/s05_jumpinterface.asm"]
    _require_order(aliases_s05, ("j_FieldMenu:", "jmp FieldMenu(pc)"), "FieldMenu alias")
    _require_order(aliases_s05, ("j_CheckArea:", "jmp CheckArea(pc)"), "CheckArea alias")
    _require_order(
        aliases,
        (
            "j_ExecuteDiamondMenu:",
            "jmp ExecuteDiamondMenu(pc)",
            "j_ExecuteMembersListScreenOnMainSummaryPage:",
            "jmp ExecuteMembersListScreenOnMainSummaryPage(pc)",
            "j_ExecuteMembersListScreenOnItemSummaryPage:",
            "jmp ExecuteMembersListScreenOnItemSummaryPage(pc)",
            "j_ExecuteMembersListScreenOnMagicSummaryPage:",
            "jmp ExecuteMembersListScreenOnMagicSummaryPage(pc)",
        ),
        "menu aliases",
    )
    _require_order(
        aliases,
        ("j_alt_YesNoPrompt:", "jmp alt_YesNoPrompt(pc)"),
        "alternate Yes/No alias",
    )

    callers: list[tuple[str, int, int]] = []
    for path, call_address in (
        ("code/gameflow/special/battletest.asm", 0x7884),
        ("code/gameflow/exploration/explorationvints.asm", 0x25BDC),
    ):
        matches = re.findall(
            r"^\s*jsr\s+j_FieldMenu\s*$", _without_comments(text[path]), re.MULTILINE
        )
        if len(matches) != 1:
            raise ValueError(f"FieldMenu caller source inventory drift: {path}")
        callers.append((path, call_address, call_address + 6))
    if len(callers) != 2:
        raise ValueError("FieldMenu direct caller denominator drift")
    instruction_target_counts = {"j_FieldMenu": len(callers)}
    effective_target_counts = {"FieldMenu": len(callers)}
    if instruction_target_counts != {"j_FieldMenu": 2} or effective_target_counts != {
        "FieldMenu": 2
    }:
        raise ValueError("FieldMenu alias-aware caller count drift")

    return {
        "sourceContext": {
            "mainactionsShape": {
                "physicalLines": 696,
                "nonEmptyLines": 679,
                "statements": 481,
                "globalLabels": 12,
                "localLabels": 46,
                "directCalls": 69,
                "directCallTargets": 33,
            }
        },
        "fieldMenuSpine": {
            "functionAddresses": {
                "FieldMenu": _FUNCTION_ADDRESSES["FieldMenu"],
                "PopulateGenericListWithCurrentForceMembers": _FUNCTION_ADDRESSES[
                    "PopulateGenericListWithCurrentForceMembers"
                ],
                "ExecuteDiamondMenu": _FUNCTION_ADDRESSES["ExecuteDiamondMenu"],
                "MembersMain": _FUNCTION_ADDRESSES["ExecuteMembersListScreenOnMainSummaryPage"],
                "MembersItem": _FUNCTION_ADDRESSES["ExecuteMembersListScreenOnItemSummaryPage"],
                "MembersMagic": _FUNCTION_ADDRESSES["ExecuteMembersListScreenOnMagicSummaryPage"],
                "BuildMemberScreen": _FUNCTION_ADDRESSES["BuildMemberScreen"],
                "AltYesNoPrompt": _FUNCTION_ADDRESSES["alt_YesNoPrompt"],
                "IsItemUsableOnField": _FUNCTION_ADDRESSES["IsItemUsableOnField"],
                "UseItemOnField": _FUNCTION_ADDRESSES["UseItemOnField"],
                "RunMapSetupItemEvent": _FUNCTION_ADDRESSES["RunMapSetupItemEvent"],
                "GetSavepointForMap": _FUNCTION_ADDRESSES["GetSavepointForMap"],
                "CheckArea": _FUNCTION_ADDRESSES["CheckArea"],
                "UpdateForce": _FUNCTION_ADDRESSES["UpdateForce"],
            },
            "jumpInterfaces": {
                "ExecuteDiamondMenu": _FUNCTION_ADDRESSES["j_ExecuteDiamondMenu"],
                "MembersMain": _FUNCTION_ADDRESSES["j_ExecuteMembersListScreenOnMainSummaryPage"],
                "MembersItem": _FUNCTION_ADDRESSES["j_ExecuteMembersListScreenOnItemSummaryPage"],
                "MembersMagic": _FUNCTION_ADDRESSES["j_ExecuteMembersListScreenOnMagicSummaryPage"],
            },
            "entryAndCallers": {
                "jumpInterfaceAddress": _FUNCTION_ADDRESSES["j_FieldMenu"],
                "instructionTarget": "j_FieldMenu",
                "effectiveTarget": "FieldMenu",
                "callerCount": len(callers),
                "instructionTargetSiteCount": instruction_target_counts["j_FieldMenu"],
                "effectiveTargetSiteCount": effective_target_counts["FieldMenu"],
                "debugCallAddress": callers[0][1],
                "debugReturnAddress": callers[0][2],
                "explorationCallAddress": callers[1][1],
                "explorationReturnAddress": callers[1][2],
            },
            "mainDispatch": {
                "menuConstant": "MENU_FIELD",
                "selectorMapping": {"member": 0, "magic": 1, "item": 2},
                "fallthrough": "search",
                "cancel": -1,
            },
            "memberAction": {
                "selector": 0,
                "listScreen": "MembersMain",
                "cancel": -1,
                "handoff": "BuildMemberScreen",
                "handoffAddress": _FUNCTION_ADDRESSES["BuildMemberScreen"],
                "return": "StartMember",
            },
            "magicAction": {
                "selector": 1,
                "listScreen": "MembersMagic",
                "cancel": -1,
                "detox": {
                    "spell": "SPELL_DETOX",
                    "spellId": enums["SPELL_DETOX"],
                    "targetListScreen": "MembersItem",
                    "levelBranches": [1, 2, "fallthrough"],
                    "callOrder": [
                        "GetSpellDefinitionAddress",
                        "DecreaseCurrentMp",
                        "GetStatusEffects",
                        "UnequipAllItemsIfNotCursed",
                        "SetStatusEffects",
                        "UpdateCombatantStats",
                    ],
                    "textIds": detox_text_ids,
                },
                "egress": {
                    "spell": "SPELL_EGRESS",
                    "spellId": enums["SPELL_EGRESS"],
                    "overworldMapRange": [
                        enums["MAP_OVERWORLD_GRANSEAL_KINGDOM"],
                        enums["MAP_OVERWORLD_PACALON_KINGDOM"],
                    ],
                    "callOrder": [
                        "GetSpellDefinitionAddress",
                        "DecreaseCurrentMp",
                        "ExecuteFlashScreenScript",
                        "GetSavepointForMap",
                    ],
                    "GetSavepointForMap": _FUNCTION_ADDRESSES["GetSavepointForMap"],
                    "postSavepointWrites": [
                        "MAP_EVENT_TYPE=1",
                        "byte=0",
                        "d0",
                        "d1",
                        "d2",
                        "d3",
                        "PLAYER_TYPE=0",
                    ],
                    "textIds": egress_text_ids,
                },
            },
            "itemAction": {
                "menuConstant": "MENU_ITEM",
                "selectorMapping": {"use": 0, "give": 1, "equip": 2, "drop": "fallthrough"},
                "cancel": -1,
                "use": {
                    "angelWing": "ITEM_ANGEL_WING",
                    "angelWingId": enums["ITEM_ANGEL_WING"],
                    "overworldMapRange": [
                        enums["MAP_OVERWORLD_GRANSEAL_KINGDOM"],
                        enums["MAP_OVERWORLD_PACALON_KINGDOM"],
                    ],
                    "angelWingCallOrder": ["RemoveItemBySlot", "Egress"],
                    "fieldUsabilityTarget": "IsItemUsableOnField",
                    "IsItemUsableOnField": _FUNCTION_ADDRESSES["IsItemUsableOnField"],
                    "usableFieldItemsAddress": usable_field_items_address,
                    "mapItemEventTarget": "RunMapSetupItemEvent",
                    "RunMapSetupItemEvent": _FUNCTION_ADDRESSES["RunMapSetupItemEvent"],
                    "targetUseTarget": "UseItemOnField",
                    "UseItemOnField": _FUNCTION_ADDRESSES["UseItemOnField"],
                    "branchOrder": ["angelWing", "fieldUsability", "mapItemEvent", "targetUse"],
                    "textIds": item_use_text_ids,
                },
                "give": {
                    "branchOrder": [
                        "selection",
                        "equipmentType",
                        "cursedGuards",
                        "recipient",
                        "give",
                        "exchange",
                    ],
                    "inventoryCapacity": enums["COMBATANT_ITEMSLOTS"],
                    "cursedGuardTargets": ["GetEquippedWeapon", "GetEquippedRing", "IsItemCursed"],
                    "exchangeCallOrder": exchange_call_order,
                    "textIds": item_give_text_ids,
                },
                "equip": {"selector": 2, "listScreen": "MembersItem", "cancel": -1},
                "drop": {
                    "unsellableMask": enums["ITEMTYPE_UNSELLABLE"],
                    "rareMask": enums["ITEMTYPE_RARE"],
                    "confirmationTarget": "alt_YesNoPrompt",
                    "AltYesNoPrompt": _FUNCTION_ADDRESSES["alt_YesNoPrompt"],
                    "confirmationAccept": 0,
                    "branchOrder": [
                        "unsellable",
                        "confirmation",
                        "cursedGuards",
                        "remove",
                        "rareDeals",
                    ],
                    "textIds": item_drop_text_ids,
                },
            },
            "searchAction": {
                "clearRegister": "d6",
                "target": "CheckArea",
                "CheckArea": _FUNCTION_ADDRESSES["CheckArea"],
                "return": "ExitMain",
            },
            "forceListHelper": {
                "UpdateForce": _FUNCTION_ADDRESSES["UpdateForce"],
                "callOrder": ["UpdateForce", "TARGETS_LIST_LENGTH", "copyBytes", "DBF"],
                "counter": "TARGETS_LIST_LENGTH-1",
                "copyOrder": "TARGETS_LIST-to-GENERIC_LIST",
            },
            "excludedUnusedTail": {
                "tableAddress": 0x21A16,
                "tableByteCount": 6,
                "helperAddress": 0x21A1C,
                "helperByteCount": 30,
            },
        },
    }


def _structural_schema() -> dict[str, Any]:
    schema = load_json(SCHEMA)
    fixture = schema.get("$defs", {}).get("fixture")
    if not isinstance(fixture, dict):
        raise ValueError("FieldMenu fixture schema definition is missing")
    return {"$schema": schema["$schema"], "$ref": "#/$defs/fixture", "$defs": schema["$defs"]}


def _validate_structural_output(value: dict[str, Any]) -> None:
    errors = sorted(
        Draft7Validator(_structural_schema()).iter_errors(value),
        key=lambda error: list(error.absolute_path),
    )
    if errors:
        location = ".".join(str(part) for part in errors[0].absolute_path) or "<root>"
        raise ValueError(
            f"FieldMenu structural schema validation failed at {location}: {errors[0].message}"
        )


def build_field_menu_control_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the deterministic FieldMenu H2 control contract; no H3 execution is involved."""
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != _ROM_SHA256:
        raise ValueError("FieldMenu canonical ROM SHA-256 drift")
    upstream = upstream_path.resolve(strict=True)
    revision = subprocess.run(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if revision != _UPSTREAM_COMMIT:
        raise ValueError("FieldMenu upstream revision drift")
    source, identities = _read_source_surface(_disasm_root(upstream))
    h1_binary = (upstream / _H1_BINARY).read_bytes()
    rom = rom_path.resolve(strict=True).read_bytes()
    addresses = listing_symbol_addresses((upstream / _LISTING).read_text(encoding="utf-8"))
    if {name: addresses.get(name) for name in _FUNCTION_ADDRESSES} != _FUNCTION_ADDRESSES:
        raise ValueError("FieldMenu H1 symbol projection drift")
    retained_owners = _retained_owners(rom_path, upstream)
    parsed = _validate_source_contract(
        source,
        usable_field_items_address=_retained_usable_field_items_address(),
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
        "system": ID,
        "summary": {"sourceFiles": 9, "h1RomAnchors": 23, "unknowns": 18},
        "retainedOwners": retained_owners,
        "sourceContext": {
            "sourceIdentities": identities,
            "h1RomAnchors": _anchor_projection(h1_binary, rom),
            **parsed["sourceContext"],
        },
        "fieldMenuSpine": parsed["fieldMenuSpine"],
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
    }
    _validate_structural_output(output)
    return output


def verify_field_menu_control_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Validate the checked-in fixture against fresh source/H1/ROM derivation."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    output = build_field_menu_control_static(rom_path, upstream_path)
    if fixture != output:
        raise ValueError("FieldMenu complete semantic fixture drift")
    return output
