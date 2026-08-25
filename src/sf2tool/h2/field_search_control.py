"""Public H2 source/H1/ROM contract for the bounded field-search control spine."""

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

ID = "sf2-field-search-control-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/field-search-control-static-v1.json")
SCHEMA = repo_path("schemas/h2/field-search-control-static-fixture.schema.json")
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
    "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
    "code/common/tech/jumpinterfaces/s05_jumpinterface.asm",
    "code/common/tech/jumpinterfaces/s07_jumpinterface.asm",
    "code/common/menus/main/mainactions.asm",
    "code/gameflow/exploration/explorationvints.asm",
    "code/gameflow/exploration/explorationfunctions_0.asm",
    "code/gameflow/exploration/explorationfunctions_1.asm",
    "code/gameflow/exploration/exploration.asm",
    "code/gameflow/battle/battlefunctions/battlefunctions_0.asm",
    "code/common/scripting/map/mapsetupsfunctions_1.asm",
    "code/common/stats/gold.asm",
    "code/common/stats/itemstats.asm",
    "code/common/stats/battleparty.asm",
    "data/stats/items/chestgoldamounts.asm",
)
_SOURCE_SHA256 = dict(
    zip(
        _SOURCE_SURFACE,
        (
            "17738776F811F66322F278CFBF10E8B376612F4F9EDFC2C7AA0A5DB81CDFB3FB",
            "ABA0DFEE4F4D3BFCD49646C03B5229DD6458FFC3A90174907823D710884020EE",
            "F5FD087710CE328C5D3DD8ADBCE2CDFD8D60F4F850743BA020F69EC56E5D63C6",
            "479D19D5CBA2D585A1AE74DD2A8CF67C455DF96FCBD2EEBB6808C6318F53A464",
            "175361E5E65A358B02705C0A4C61D55500209429F88D2C1FAEF4F4113E6975CE",
            "A7884F9F6B4E465FEBD531BA5B538EDB1FF5BC93FBB67DFCFAED7BFB208F25AE",
            "E69B3D40DF6658CF761A035FA1B6643DD9770B3C82F4CBB21D5AC4D3A50F2777",
            "977A8D25FC1F8B155A5AC8D7360A6513C0C7A76ADCCC5C6A29EB55D34DB66D3B",
            "BBFED0DF86D68A813047650B3E10754382881C17B2F644F8F1BC897F77EA3B4E",
            "E122508EBC4089056F4B5E6788520AB6DB213B0AE829042F0E88C7BC0B572907",
            "C38279815C832B5D65B443092048BB92E19FAEE47B81734A3EF0D16AA0E445A0",
            "706732153061EE4846FE15B544F72C2BEEB29E0FC12F42BC959664FA470E3B76",
            "F99AF47A35E9148176E66865694B2BCEE3D3DF8C6013227F5CB8ECA330AC6531",
            "E578316F1E5CBA89AF55B6E5FDC7022B08669DD93FC7564ED73EB065D2D4A88E",
            "DBDB7320F86EA8A24C1E2684631EAFCD3A4F92A95C6EFDC7E3A3F579672CBC7B",
            "670A25075D807BA60B0AA3C6D158DDF80E5248264753361DBC495F7655ED8B37",
            "BA250BBD5ADA3BD7DCD181B6CF0BE07FAF7C600DFA8CCF3F44E18C4C40E9FCA0",
        ),
        strict=True,
    )
)

_FUNCTION_ADDRESSES = {
    "j_CheckArea": 0x2004C,
    "CheckArea": 0x23862,
    "GetChestGoldAmount": 0x2399C,
    "table_ChestGoldAmounts": 0x239AE,
    "itemHandoff": 0x239C8,
    "OpenChest": 0x4156,
    "CloseChest": 0x4194,
    "CheckNonChestItem": 0x41F6,
    "RefillNonChestItem": 0x421A,
    "FadeOut_WaitForP1Input": 0x23758,
    "j_RunMapSetupAreaDescription": 0x440B4,
    "RunMapSetupAreaDescription": 0x47702,
    "j_IncreaseGold": 0x815C,
    "IncreaseGold": 0x899A,
    "j_GetItemBySlotAndHeldItemsNumber": 0x8174,
    "GetItemBySlotAndHeldItemsNumber": 0x8BFA,
    "j_AddItem": 0x8198,
    "AddItem": 0x8CA2,
    "j_UpdateForce": 0x8270,
    "UpdateForce": 0x9900,
}

_ANCHORS = (
    ("callers.fieldMenu.call", 0x219DE, 6),
    ("callers.processPlayerAction.noEntityCall", 0x25BC2, 6),
    ("functionAddresses.j_CheckArea", 0x2004C, 4),
    ("functionAddresses.CheckArea", 0x23862, 314),
    ("functionAddresses.GetChestGoldAmount", 0x2399C, 18),
    ("goldPath.table", 0x239AE, 26),
    ("itemRecipientPath.entry", 0x239C8, 188),
    ("blockDispatch.OpenChest", 0x4156, 2),
    ("fullInventoryRollback.CloseChest", 0x4194, 2),
    ("blockDispatch.CheckNonChestItem", 0x41F6, 2),
    ("fullInventoryRollback.RefillNonChestItem", 0x421A, 2),
    ("itemRecipientPath.FadeOut_WaitForP1Input", 0x23758, 2),
    ("areaDescriptionFallback.j_RunMapSetupAreaDescription", 0x440B4, 4),
    ("areaDescriptionFallback.RunMapSetupAreaDescription", 0x47702, 2),
    ("goldPath.j_IncreaseGold", 0x815C, 4),
    ("goldPath.IncreaseGold", 0x899A, 2),
    ("itemRecipientPath.j_GetItemBySlotAndHeldItemsNumber", 0x8174, 4),
    ("itemRecipientPath.GetItemBySlotAndHeldItemsNumber", 0x8BFA, 2),
    ("itemRecipientPath.j_AddItem", 0x8198, 4),
    ("itemRecipientPath.AddItem", 0x8CA2, 2),
    ("itemRecipientPath.j_UpdateForce", 0x8270, 4),
    ("itemRecipientPath.UpdateForce", 0x9900, 2),
)

_UNKNOWN_KEYS = (
    "natural-search-reachability",
    "actual-caller-entry-state",
    "actual-view-target-entity",
    "actual-facing-and-target-coordinate",
    "actual-block-kind",
    "actual-area-description-row-or-callback",
    "actual-chest-or-nonchest-content",
    "actual-gold-before-and-after",
    "actual-item-recipient-and-capacity",
    "actual-item-flag-open-close-refill-state",
    "actual-return-code-and-caller-branch",
    "input-text-sound-and-fade-cadence",
    "persistence-after-map-switch-save-load",
    "route-specific-search-outcome",
)

_RETAINED_OWNER_PATHS = {
    "gameflowCore": "tests/fixtures/h2/gameflow-core-static-v1.json",
    "fieldMenuControl": "tests/fixtures/h2/field-menu-control-static-v1.json",
    "mapDescriptions": "tests/fixtures/h2/map-descriptions-static-v1.json",
    "commonStats": "tests/fixtures/h2/common-stats-static-v1.json",
    "coreStatsData": "tests/fixtures/h2/core-stats-data-static-v1.json",
    "itemAuxiliary": "tests/fixtures/h2/item-auxiliary-static-v1.json",
    "techInterfaces": "tests/fixtures/h2/tech-interfaces-static-v1.json",
}
_RETAINED_OWNER_IDS = {
    "gameflowCore": "sf2-gameflow-core-static-v1",
    "fieldMenuControl": "sf2-field-menu-control-static-v1",
    "mapDescriptions": "sf2-map-descriptions-static-v1",
    "commonStats": "sf2-common-stats-static-v1",
    "coreStatsData": "sf2-core-stats-data-static-v1",
    "itemAuxiliary": "sf2-item-auxiliary-static-v1",
    "techInterfaces": "sf2-tech-interfaces-static-v1",
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
            raise ValueError(f"Field search control {context} source-use drift: {fragment}")
        cursor = found + len(fragment)


def _source_region(source: str, start: str, end: str, context: str) -> str:
    start_at = source.find(start)
    if start_at < 0:
        raise ValueError(f"Field search control {context} start drift: {start}")
    end_at = source.find(end, start_at + len(start))
    if end_at < 0:
        raise ValueError(f"Field search control {context} end drift: {end}")
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


def _direct_calls(source: str) -> list[str]:
    """Parse direct jsr/bsr targets, excluding labels, comments, and near misses."""
    calls: list[str] = []
    pattern = re.compile(
        r"^\s*(?:jsr|bsr)(?:\.[sbwl])?\s+(?:\()?([A-Za-z_][A-Za-z0-9_]*)"
        r"(?:\([^)]*\))?(?:\)?(?:\.[wl])?)?\s*$"
    )
    for raw in _without_comments(source).splitlines():
        match = pattern.match(raw)
        if match is not None:
            calls.append(match.group(1))
    return calls


def _read_source_surface(root: Path) -> tuple[dict[str, str], list[dict[str, str]]]:
    text: dict[str, str] = {}
    identities: list[dict[str, str]] = []
    for relative in _SOURCE_SURFACE:
        path = root / relative
        if not path.is_file():
            raise ValueError(f"Field search control source is missing: {relative}")
        data = path.read_bytes()
        sha256 = hashlib.sha256(data).hexdigest().upper()
        if sha256 != _SOURCE_SHA256[relative]:
            raise ValueError(f"Field search control source hash drift: {relative}")
        identities.append({"path": relative, "sha256": sha256})
        text[relative] = data.decode("utf-8").replace("\r\n", "\n")
    if len(identities) != 17:
        raise ValueError("Field search control source denominator drift")
    return text, identities


def _anchor_projection(h1_binary: bytes, rom: bytes) -> list[dict[str, int | str]]:
    anchors: list[dict[str, int | str]] = []
    for identifier, address, width in _ANCHORS:
        h1 = h1_binary[address : address + width]
        rom_bytes = rom[address : address + width]
        if len(h1) != width or len(rom_bytes) != width or h1 != rom_bytes:
            raise ValueError(f"Field search control H1/ROM anchor drift: {identifier}")
        anchors.append(
            {
                "id": identifier,
                "address": address,
                "endAddressExclusive": address + width,
                "byteLength": width,
                "sha256": hashlib.sha256(rom_bytes).hexdigest().upper(),
            }
        )
    if len(anchors) != 22:
        raise ValueError("Field search control H1/ROM anchor denominator drift")
    return anchors


def _retained_owners() -> dict[str, dict[str, str]]:
    owners: dict[str, dict[str, str]] = {}
    for name, relative in _RETAINED_OWNER_PATHS.items():
        path = repo_path(relative)
        fixture = load_json(path)
        if fixture.get("id") != _RETAINED_OWNER_IDS[name]:
            raise ValueError(f"Field search control retained owner identity drift: {name}")
        owners[name] = {
            "fixtureId": fixture["id"],
            "fixtureSha256": hashlib.sha256(path.read_bytes()).hexdigest().upper(),
        }
    if set(owners) != set(_RETAINED_OWNER_PATHS) or len(owners) != 7:
        raise ValueError("Field search control retained owner denominator drift")
    return owners


def _guard_retained_projections() -> None:
    """Reject retained source-shape joins before accepting this narrower spine."""
    field_menu = load_json(repo_path(_RETAINED_OWNER_PATHS["fieldMenuControl"]))
    if field_menu["fieldMenuSpine"]["searchAction"] != {
        "clearRegister": "d6",
        "target": "CheckArea",
        "CheckArea": _FUNCTION_ADDRESSES["CheckArea"],
        "return": "ExitMain",
    }:
        raise ValueError("Field search control retained FieldMenu projection drift")
    descriptions = load_json(repo_path(_RETAINED_OWNER_PATHS["mapDescriptions"]))
    consumer = descriptions["expected"]["consumerFacts"]
    if (
        consumer["normalExplorationD6Value"] != 1
        or not consumer["nonzeroConditionByteRequiresZeroD6"]
    ):
        raise ValueError("Field search control retained map-description projection drift")
    tech_interfaces = load_json(repo_path(_RETAINED_OWNER_PATHS["techInterfaces"]))
    expected = tech_interfaces["expected"]
    required_sources = {
        "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
        "code/common/tech/jumpinterfaces/s05_jumpinterface.asm",
        "code/common/tech/jumpinterfaces/s07_jumpinterface.asm",
    }
    if (
        not required_sources <= set(expected["indexedSourcePaths"])
        or expected["interfaceFacts"]["jumpStubCount"] != 331
    ):
        raise ValueError("Field search control retained jump-interface projection drift")


def _text_ids(source: str, start: str, end: str, context: str) -> list[int]:
    region = _source_region(source, start, end, context)
    values = [
        int(value)
        for value in re.findall(r"^\s*txt\s+(\d+)\b", _without_comments(region), re.MULTILINE)
    ]
    if not values:
        raise ValueError(f"Field search control {context} text-ID use-site drift")
    return values


def _validate_source_contract(text: dict[str, str]) -> dict[str, Any]:
    """Parse the fixed CheckArea caller, dispatch, and recovery spine without H3 claims."""
    constants = _parse_equ(text["sf2const.asm"])
    constants.update(_parse_equ(text["sf2enums.asm"]))
    required_constants = {
        "MAP_TILE_SIZE": 0x180,
        "INDEX_SHIFT_COUNT": 2,
        "ITEMENTRY_MASK_INDEX": 0x7F,
        "ITEM_NOTHING": 127,
        "ITEMINDEX_GOLDCHESTS_START": 128,
        "COMBATANT_ITEMSLOTS": 4,
    }
    if {key: constants.get(key) for key in required_constants} != required_constants:
        raise ValueError("Field search control authoritative constants drift")

    layout = text["layout/sf2-05-0x020000-0x028000.asm"]
    _require_order(
        layout,
        (
            "code\\gameflow\\exploration\\explorationfunctions_0.asm",
            "data\\stats\\items\\chestgoldamounts.asm",
            "code\\gameflow\\exploration\\explorationfunctions_1.asm",
        ),
        "canonical layout order",
    )

    aliases_s02 = text["code/common/tech/jumpinterfaces/s02_jumpinterface.asm"]
    aliases_s05 = text["code/common/tech/jumpinterfaces/s05_jumpinterface.asm"]
    aliases_s07 = text["code/common/tech/jumpinterfaces/s07_jumpinterface.asm"]
    _require_order(aliases_s05, ("j_CheckArea:", "jmp CheckArea(pc)"), "CheckArea alias")
    _require_order(
        aliases_s07,
        ("j_RunMapSetupAreaDescription:", "jmp RunMapSetupAreaDescription(pc)"),
        "area-description alias",
    )
    for alias, target in (
        ("j_IncreaseGold", "IncreaseGold"),
        ("j_GetItemBySlotAndHeldItemsNumber", "GetItemBySlotAndHeldItemsNumber"),
        ("j_AddItem", "AddItem"),
        ("j_UpdateForce", "UpdateForce"),
    ):
        _require_order(aliases_s02, (f"{alias}:", f"jmp {target}(pc)"), f"{alias} alias")

    # These retained services own their full algorithms. This narrower contract
    # guards only the exact effective service boundary consumed by CheckArea.
    exploration_services = text["code/gameflow/exploration/exploration.asm"]
    _require_order(
        _source_region(
            exploration_services, "OpenChest:", "End of function OpenChest", "OpenChest service"
        ),
        ("OpenChest:", "bsr.w GetChestItem", "jsr j_CheckFlag", "jsr j_SetFlag"),
        "OpenChest effective service",
    )
    _require_order(
        _source_region(
            exploration_services,
            "CloseChest:",
            "End of function CloseChest",
            "CloseChest service",
        ),
        ("CloseChest:", "bsr.w GetChestItem", "jsr j_ClearFlag"),
        "CloseChest effective service",
    )
    _require_order(
        _source_region(
            exploration_services,
            "CheckNonChestItem:",
            "End of function CheckNonChestItem",
            "CheckNonChestItem service",
        ),
        ("CheckNonChestItem:", "bsr.w GetNonChestItem", "jsr j_CheckFlag", "jsr j_SetFlag"),
        "CheckNonChestItem effective service",
    )
    _require_order(
        _source_region(
            exploration_services,
            "RefillNonChestItem:",
            "End of function RefillNonChestItem",
            "RefillNonChestItem service",
        ),
        ("RefillNonChestItem:", "bsr.w GetNonChestItem", "jsr j_ClearFlag"),
        "RefillNonChestItem effective service",
    )
    _require_order(
        _source_region(
            text["code/common/scripting/map/mapsetupsfunctions_1.asm"],
            "RunMapSetupAreaDescription:",
            "End of function RunMapSetupAreaDescription",
            "area-description service",
        ),
        ("RunMapSetupAreaDescription:", "bsr.w GetCurrentMapSetup", "tst.w d7", "rts"),
        "area-description effective service",
    )
    _require_order(
        _source_region(
            text["code/common/stats/gold.asm"],
            "IncreaseGold:",
            "End of function IncreaseGold",
            "IncreaseGold service",
        ),
        ("IncreaseGold:", "add.l ((CURRENT_GOLD-$1000000)).w,d1", "rts"),
        "IncreaseGold effective service",
    )
    _require_order(
        _source_region(
            text["code/common/stats/itemstats.asm"],
            "GetItemBySlotAndHeldItemsNumber:",
            "End of function GetItemBySlotAndHeldItemsNumber",
            "GetItemBySlotAndHeldItemsNumber service",
        ),
        ("GetItemBySlotAndHeldItemsNumber:", "moveq #COMBATANT_ITEMSLOTS_COUNTER,d3"),
        "GetItemBySlotAndHeldItemsNumber effective service",
    )
    _require_order(
        _source_region(
            text["code/common/stats/itemstats.asm"],
            "AddItem:",
            "End of function AddItem",
            "AddItem service",
        ),
        ("AddItem:", "moveq #COMBATANT_ITEMSLOTS_COUNTER,d0"),
        "AddItem effective service",
    )
    _require_order(
        _source_region(
            text["code/common/stats/battleparty.asm"],
            "UpdateForce:",
            "End of function UpdateForce",
            "UpdateForce service",
        ),
        ("UpdateForce:", "lea ((TARGETS_LIST-$1000000)).w,a2", "TARGETS_LIST_LENGTH"),
        "UpdateForce effective service",
    )
    _require_order(
        _source_region(
            text["code/gameflow/battle/battlefunctions/battlefunctions_0.asm"],
            "FadeOut_WaitForP1Input:",
            "End of function FadeOut_WaitForP1Input",
            "input-wait service",
        ),
        (
            "FadeOut_WaitForP1Input:",
            "jsr (PlayMusicAfterCurrentOne).w",
            "jsr (WaitForPlayerInput).w",
        ),
        "input-wait effective service",
    )

    main = text["code/common/menus/main/mainactions.asm"]
    actions = text["code/gameflow/exploration/explorationvints.asm"]
    main_search = _source_region(
        main,
        "@SearchAction:",
        "PopulateGenericListWithCurrentForceMembers:",
        "FieldMenu caller",
    )
    _require_order(
        main_search,
        ("@SearchAction:", "clr.w d6", "jsr j_CheckArea", "bra.w @ExitMain"),
        "FieldMenu caller",
    )
    no_entity = _source_region(
        actions, "loc_25BC0:", "loc_25BCC:", "ProcessPlayerAction no-entity caller"
    )
    _require_order(
        no_entity,
        ("moveq #1,d6", "jsr CheckArea", "bne.w return_25BF2"),
        "ProcessPlayerAction no-entity caller",
    )
    field_menu_calls = _direct_calls(main_search)
    process_calls = _direct_calls(no_entity)
    if field_menu_calls != ["j_CheckArea"] or process_calls != ["CheckArea"]:
        raise ValueError("Field search control caller instruction inventory drift")
    caller_inventory = {
        "instructionTargetSiteCounts": {
            "j_CheckArea": len(field_menu_calls),
            "CheckArea": len(process_calls),
        },
        "effectiveTargetSiteCounts": {"CheckArea": len(field_menu_calls) + len(process_calls)},
    }
    if caller_inventory != {
        "instructionTargetSiteCounts": {"j_CheckArea": 1, "CheckArea": 1},
        "effectiveTargetSiteCounts": {"CheckArea": 2},
    }:
        raise ValueError("Field search control alias-aware caller inventory drift")

    check_area = text["code/gameflow/exploration/explorationfunctions_0.asm"]
    coordinate = _source_region(
        check_area, "CheckArea:", "cmpi.w  #$1800,d3", "target-coordinate derivation"
    )
    _require_order(
        coordinate,
        (
            "CheckArea:",
            "move.b ((VIEW_TARGET_ENTITY-$1000000)).w,d0",
            "ext.w d0",
            "bpl.s loc_2386C",
            "rts",
            "lea ((ENTITY_DATA-$1000000)).w,a0",
            "lsl.w #ENTITYDEF_SIZE_BITS,d0",
            "adda.w d0,a0",
            "move.w (a0,d0.w),d2",
            "move.w ENTITYDEF_OFFSET_Y(a0,d0.w),d1",
            "move.b ENTITYDEF_OFFSET_FACING(a0,d0.w),d3",
            "move.w d2,d0",
            "andi.w #3,d3",
            "move.w d3,d5",
            "lsl.w #INDEX_SHIFT_COUNT,d5",
            "lea table_PixelOffsets_X(pc),a0",
            "add.w (a0,d5.w),d0",
            "add.w 2(a0,d5.w),d1",
            "ext.l d0",
            "ext.l d1",
            "divs.w #MAP_TILE_SIZE,d0",
            "divs.w #MAP_TILE_SIZE,d1",
            "move.w d0,d4",
            "move.w d1,d5",
            "move.w d1,d3",
            "lsl.w #6,d3",
            "add.w d0,d3",
            "add.w d3,d3",
            "lea (FF0000_RAM_START).l,a0",
            "move.w (a0,d3.w),d3",
            "andi.w #$3C00,d3",
        ),
        "target-coordinate derivation",
    )
    direction_mask_match = re.search(r"(?m)^\s*andi\.w\s+#(\d+),d3\s*$", coordinate)
    layout_shift_match = re.search(r"(?m)^\s*lsl\.w\s+#(\d+),d3\s*$", coordinate)
    block_mask_match = re.search(r"(?m)^\s*andi\.w\s+#\$([0-9A-F]+),d3\s*$", coordinate)
    if direction_mask_match is None or layout_shift_match is None or block_mask_match is None:
        raise ValueError("Field search control target-coordinate operand drift")
    direction_mask = int(direction_mask_match.group(1))
    layout_shift = int(layout_shift_match.group(1))
    block_mask = int(block_mask_match.group(1), 16)
    if direction_mask != 3 or layout_shift != 6 or block_mask != 0x3C00:
        raise ValueError("Field search control target-coordinate operand value drift")
    dispatch_specs = (
        ("chest", "CheckArea:", "$1800", "OpenChest", 403, 408, "loc_238E8"),
        ("vase", "loc_238E8:", "$2C00", "CheckNonChestItem", 404, 409, "loc_2390C"),
        ("barrel", "loc_2390C:", "$3000", "CheckNonChestItem", 405, 410, "loc_23930"),
        ("bookshelf", "loc_23930:", "$3400", "CheckNonChestItem", 427, 412, "loc_23954"),
        (
            "genericSearchable",
            "loc_23954:",
            "$1C00",
            "CheckNonChestItem",
            423,
            412,
            "loc_23978",
        ),
    )
    block_kinds: list[dict[str, Any]] = []
    for (
        kind,
        start_label,
        mask_literal,
        target,
        action_text,
        empty_text,
        next_label,
    ) in dispatch_specs:
        block = _source_region(check_area, start_label, f"{next_label}:", f"{kind} block")
        _require_order(
            block,
            (
                f"cmpi.w #{mask_literal},d3",
                f"bne.s {next_label}",
                f"jsr ({target}).w",
                f"txt {action_text}",
                "move.w d2,d0",
                "andi.w #ITEMENTRY_MASK_INDEX,d0",
                "cmpi.b #ITEM_NOTHING,d0",
                "bne.w loc_239C8",
                f"txt {empty_text}",
                "bra.w byte_23994",
            ),
            f"{kind} block dispatch",
        )
        block_kinds.append(
            {
                "kind": kind,
                "mask": int(mask_literal[1:], 16),
                "contentTarget": target,
                "actionTextId": action_text,
                "emptyTextId": empty_text,
            }
        )
    if [row["mask"] for row in block_kinds] != [0x1800, 0x2C00, 0x3000, 0x3400, 0x1C00]:
        raise ValueError("Field search control block dispatch order drift")

    _require_order(
        check_area,
        (
            "loc_23978:",
            "jsr j_RunMapSetupAreaDescription",
            "bne.w byte_23994",
            "tst.w d6",
            "beq.s byte_2398C",
            "clr.w d0",
            "bra.w return_2399A",
            "byte_2398C:",
            "txt 423",
            "txt 412",
            "byte_23994:",
            "clsTxt",
            "moveq #-1,d0",
            "return_2399A:",
            "rts",
        ),
        "area-description fallback polarity",
    )

    gold_function = _source_region(
        check_area,
        "GetChestGoldAmount:",
        "End of function GetChestGoldAmount",
        "gold index",
    )
    _require_order(
        gold_function,
        (
            "subi.w #ITEMINDEX_GOLDCHESTS_START,d2",
            "andi.w #ITEMENTRY_MASK_INDEX,d2",
            "add.w d2,d2",
            "move.w table_ChestGoldAmounts(pc,d2.w),d1",
            "ext.l d1",
        ),
        "gold index transform",
    )
    table = text["data/stats/items/chestgoldamounts.asm"]
    rows = re.findall(r"^\s*dc\.w\s+(\d+)\s*$", _without_comments(table), re.MULTILINE)
    gold_values = [int(value) for value in rows]
    if gold_values != list(range(10, 131, 10)):
        raise ValueError("Field search control chest-gold table values drift")

    handoff = text["code/gameflow/exploration/explorationfunctions_1.asm"]
    gold_handoff = _source_region(handoff, "loc_239C8:", "loc_239EE:", "gold handoff")
    _require_order(
        gold_handoff,
        (
            "loc_239C8:",
            "clr.w d0",
            "move.w d0,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
            "cmpi.w #ITEMINDEX_GOLDCHESTS_START,d2",
            "blt.s loc_239EE",
            "bsr.s GetChestGoldAmount",
            "jsr j_IncreaseGold",
            "sndCom MUSIC_ITEM",
            "txt 414",
            "bsr.w FadeOut_WaitForP1Input",
            "bra.s byte_23994",
        ),
        "gold handoff return",
    )
    leader_handoff = _source_region(handoff, "loc_239EE:", "loc_23A1E:", "leader handoff")
    _require_order(
        leader_handoff,
        (
            "loc_239EE:",
            "move.w d2,((DIALOGUE_NAME_INDEX_2-$1000000)).w",
            "txt 413",
            "clr.w d1",
            "jsr j_GetItemBySlotAndHeldItemsNumber",
            "cmpi.w #COMBATANT_ITEMSLOTS,d2",
            "bge.s loc_23A1E",
            "move.w ((DIALOGUE_NAME_INDEX_2-$1000000)).w,d1",
            "jsr j_AddItem",
            "sndCom MUSIC_ITEM",
            "txt 415",
            "bsr.w FadeOut_WaitForP1Input",
            "bra.w byte_23994",
        ),
        "leader item handoff",
    )
    member_handoff = _source_region(handoff, "loc_23A32:", "loc_23A62:", "member handoff")
    _require_order(
        member_handoff,
        (
            "loc_23A32:",
            "clr.w d0",
            "move.b (a0)+,d0",
            "clr.w d1",
            "jsr j_GetItemBySlotAndHeldItemsNumber",
            "cmpi.w #COMBATANT_ITEMSLOTS,d2",
            "bge.s loc_23A62",
            "move.w ((DIALOGUE_NAME_INDEX_2-$1000000)).w,d1",
            "jsr j_AddItem",
            "move.w d0,((DIALOGUE_NAME_INDEX_3-$1000000)).w",
            "sndCom MUSIC_ITEM",
            "txt 416",
            "bsr.w FadeOut_WaitForP1Input",
            "bra.w byte_23994",
        ),
        "member item handoff",
    )
    full_inventory = _source_region(
        handoff,
        "loc_23A66:",
        "END OF FUNCTION CHUNK FOR CheckArea",
        "full-inventory rollback",
    )
    _require_order(
        full_inventory,
        (
            "loc_23A66:",
            "move.w ((DIALOGUE_NAME_INDEX_2-$1000000)).w,d3",
            "clr.w d0",
            "move.w d0,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
            "txt 417",
            "move.w d4,d0",
            "move.w d5,d1",
            "jsr (CloseChest).w",
            "jsr (RefillNonChestItem).w",
            "bra.w byte_23994",
        ),
        "full-inventory rollback",
    )
    recipient_loop = _source_region(handoff, "loc_23A1E:", "loc_23A66:", "member recipient loop")
    _require_order(
        recipient_loop,
        (
            "loc_23A1E:",
            "jsr j_UpdateForce",
            "lea ((OTHER_FORCE_MEMBERS_LIST-$1000000)).w,a0",
            "move.w ((TARGETS_LIST_LENGTH-$1000000)).w,d7",
            "subq.w #2,d7",
            "bmi.w loc_23A66",
            "loc_23A62:",
            "dbf d7,loc_23A32",
        ),
        "member recipient loop",
    )

    public_text_ids = _text_ids(
        check_area, "CheckArea:", "End of function CheckArea", "CheckArea"
    ) + _text_ids(
        handoff,
        "loc_239C8:",
        "END OF FUNCTION CHUNK FOR CheckArea",
        "item handoff",
    )
    expected_public_text_ids = [
        403,
        408,
        404,
        409,
        405,
        410,
        427,
        412,
        423,
        412,
        423,
        412,
        414,
        413,
        415,
        416,
        417,
    ]
    if public_text_ids != expected_public_text_ids:
        raise ValueError("Field search control public text-ID use-site drift")

    return {
        "sourceContext": {
            "layoutIncludes": [
                "code\\gameflow\\exploration\\explorationfunctions_0.asm",
                "data\\stats\\items\\chestgoldamounts.asm",
                "code\\gameflow\\exploration\\explorationfunctions_1.asm",
            ],
            "callerInventory": caller_inventory,
        },
        "fieldSearchSpine": {
            "callers": {
                "fieldMenu": {
                    "callAddress": 0x219DE,
                    "returnAddress": 0x219E4,
                    "instructionTarget": "j_CheckArea",
                    "effectiveTarget": "CheckArea",
                    "d6Value": 0,
                    "returnTarget": "ExitMain",
                },
                "processPlayerActionNoEntity": {
                    "callAddress": 0x25BC2,
                    "returnAddress": 0x25BC8,
                    "instructionTarget": "CheckArea",
                    "effectiveTarget": "CheckArea",
                    "d6Value": 1,
                    "nonzeroBranch": "return_25BF2",
                },
            },
            "functionAddresses": _FUNCTION_ADDRESSES,
            "targetCoordinate": {
                "viewTargetEntity": "VIEW_TARGET_ENTITY",
                "negativeBranch": "bpl",
                "negativeReturn": "rts",
                "entityScaleShift": "ENTITYDEF_SIZE_BITS",
                "directionMask": direction_mask,
                "directionOffsetIndexShift": constants["INDEX_SHIFT_COUNT"],
                "pixelOffsetTable": "table_PixelOffsets_X",
                "tileSize": constants["MAP_TILE_SIZE"],
                "layoutRowWidth": 1 << layout_shift,
                "layoutWordScale": 2,
                "layoutBase": "FF0000_RAM_START",
                "blockMask": block_mask,
            },
            "blockDispatch": {
                "itemEntryMask": constants["ITEMENTRY_MASK_INDEX"],
                "itemNothing": constants["ITEM_NOTHING"],
                "kinds": block_kinds,
            },
            "areaDescriptionFallback": {
                "instructionTarget": "j_RunMapSetupAreaDescription",
                "effectiveTarget": "RunMapSetupAreaDescription",
                "handledBranch": "bne",
                "d6Test": "tst",
                "d6OneReturn": 0,
                "d6ZeroDefaultTextIds": [423, 412],
                "defaultReturn": -1,
                "closeText": "clsTxt",
            },
            "contentClassification": {
                "goldChestStart": constants["ITEMINDEX_GOLDCHESTS_START"],
                "goldBranch": "bge",
                "itemBranch": "blt",
                "itemEntryMask": constants["ITEMENTRY_MASK_INDEX"],
                "itemNothing": constants["ITEM_NOTHING"],
            },
            "goldPath": {
                "functionAddress": _FUNCTION_ADDRESSES["GetChestGoldAmount"],
                "tableAddress": _FUNCTION_ADDRESSES["table_ChestGoldAmounts"],
                "indexTransform": [
                    "subtractGoldChestStart",
                    "maskItemEntryIndex",
                    "doubleWordIndex",
                ],
                "tableValues": gold_values,
                "increaseGoldInstructionTarget": "j_IncreaseGold",
                "increaseGoldEffectiveTarget": "IncreaseGold",
                "callOrder": [
                    "GetChestGoldAmount",
                    "IncreaseGold",
                    "MUSIC_ITEM",
                    "txt414",
                    "FadeOut_WaitForP1Input",
                ],
            },
            "itemRecipientPath": {
                "leaderIndex": 0,
                "inventoryCapacity": constants["COMBATANT_ITEMSLOTS"],
                "leaderFirst": True,
                "forceUpdateInstructionTarget": "j_UpdateForce",
                "forceUpdateEffectiveTarget": "UpdateForce",
                "recipientList": "OTHER_FORCE_MEMBERS_LIST",
                "counter": "TARGETS_LIST_LENGTH-2",
                "loop": "DBF",
                "firstEligibleAction": "AddItem",
                "textIds": [413, 415, 416],
            },
            "fullInventoryRollback": {
                "textId": 417,
                "callOrder": ["CloseChest", "RefillNonChestItem"],
                "returnTarget": "byte_23994",
            },
            "returnContract": {
                "negativeViewTarget": "rts",
                "areaDescriptionHandled": -1,
                "areaDescriptionUnhandledD6One": 0,
                "areaDescriptionUnhandledD6Zero": -1,
                "processPlayerActionNonzeroBranch": "return_25BF2",
            },
            "publicTextIds": public_text_ids,
        },
    }


def _structural_schema() -> dict[str, Any]:
    schema = load_json(SCHEMA)
    fixture = schema.get("$defs", {}).get("fixture")
    if not isinstance(fixture, dict):
        raise ValueError("Field search control fixture schema definition is missing")
    return {"$schema": schema["$schema"], "$ref": "#/$defs/fixture", "$defs": schema["$defs"]}


def _validate_structural_output(value: dict[str, Any]) -> None:
    errors = sorted(
        Draft7Validator(_structural_schema()).iter_errors(value),
        key=lambda error: list(error.absolute_path),
    )
    if errors:
        location = ".".join(str(part) for part in errors[0].absolute_path) or "<root>"
        raise ValueError(
            "Field search control structural schema validation failed at "
            f"{location}: {errors[0].message}"
        )


def build_field_search_control_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the deterministic public H2 field-search contract; no H3 execution occurs."""
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != _ROM_SHA256:
        raise ValueError("Field search control canonical ROM SHA-256 drift")
    upstream = upstream_path.resolve(strict=True)
    revision = subprocess.run(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if revision != _UPSTREAM_COMMIT:
        raise ValueError("Field search control upstream revision drift")
    source, identities = _read_source_surface(_disasm_root(upstream))
    h1_binary = (upstream / _H1_BINARY).read_bytes()
    rom = rom_path.resolve(strict=True).read_bytes()
    addresses = listing_symbol_addresses((upstream / _LISTING).read_text(encoding="utf-8"))
    expected_symbols = {
        name: address
        for name, address in _FUNCTION_ADDRESSES.items()
        if name not in {"itemHandoff"}
    }
    if {name: addresses.get(name) for name in expected_symbols} != expected_symbols:
        raise ValueError("Field search control H1 symbol projection drift")
    _guard_retained_projections()
    parsed = _validate_source_contract(source)
    toolchain = load_json(TOOLCHAIN)
    output = {
        "schemaVersion": 1,
        "id": ID,
        "system": ID,
        "romSha256": load_json(ROM_MANIFEST)["hashes"]["sha256"],
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": toolchain["sf2disasm"]["commit"],
        },
        "sourceContext": {
            "sourceIdentities": identities,
            "h1RomAnchors": _anchor_projection(h1_binary, rom),
            **parsed["sourceContext"],
        },
        "retainedOwners": _retained_owners(),
        "fieldSearchSpine": parsed["fieldSearchSpine"],
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": {"sourceFiles": 17, "h1RomAnchors": 22, "callers": 2, "unknowns": 14},
    }
    _validate_structural_output(output)
    return output


def verify_field_search_control_static(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Validate the checked-in fixture against fresh source/H1/ROM derivation."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    output = build_field_search_control_static(rom_path, upstream_path)
    if fixture != output:
        raise ValueError("Field search control complete semantic fixture drift")
    return output
