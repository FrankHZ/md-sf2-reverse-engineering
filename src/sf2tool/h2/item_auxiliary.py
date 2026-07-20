from __future__ import annotations

import hashlib
import json
from fractions import Fraction
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_global_data import _arguments, _byte_values, _integer, _tokens
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h3.growth import _parse_equates
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom
from sf2tool.source_text import read_upstream_text

ID = "sf2-item-auxiliary-static-v1"
MANIFEST = repo_path("manifests/extractions/item-auxiliary-static.json")
SCHEMA = repo_path("schemas/item-auxiliary-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/item-auxiliary-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-item-auxiliary-static-fixture.schema.json")

SOURCE_PATHS = {
    "shops": Path("data/stats/items/shopinventories.asm"),
    "debugShop": Path("data/stats/items/debugshop.asm"),
    "chestGold": Path("data/stats/items/chestgoldamounts.asm"),
    "breakMessages": Path("data/stats/items/itembreakmessages.asm"),
    "mithril": Path("data/stats/items/mithrilweapons.asm"),
    "specialCaravan": Path("data/stats/items/specialcaravandescriptions.asm"),
    "fieldItems": Path("data/stats/items/usableoutsidebattleitems.asm"),
    "weaponGraphics": Path("data/stats/items/weapongraphics.asm"),
}

CONSUMER_PATHS = {
    "shops": Path("code/common/menus/shop/shopactions.asm"),
    "mithril": Path("code/common/menus/blacksmith/pickmithrilweapon.asm"),
    "chestGold": Path("code/gameflow/exploration/explorationfunctions_0.asm"),
    "breakMessages": Path("code/gameflow/battle/battleactions/breakuseditem.asm"),
    "specialCaravan": Path("code/common/menus/caravan/caravanactions_1.asm"),
    "fieldItems": Path("code/common/menus/item/isitemusableonfield.asm"),
    "weaponGraphics": Path(
        "code/gameflow/battle/battlescenes/getweaponspriteandpalette.asm"
    ),
}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _resolve_token(token: str, prefix: str, equates: dict[str, int]) -> int:
    token = token.strip()
    try:
        return _integer(token)
    except ValueError:
        name = f"{prefix}{token}"
        if name not in equates:
            raise ValueError(f"unknown source token: {name}") from None
        return equates[name]


def _named_value(code: str, prefix: str, equates: dict[str, int]) -> dict[str, Any]:
    return {"code": code, "value": _resolve_token(code, prefix, equates)}


def _assert_consumer(source: str, owner: str, fragments: tuple[str, ...]) -> None:
    missing = [fragment for fragment in fragments if fragment not in source]
    if missing:
        raise ValueError(f"{owner} consumer contract drift: {missing}")


def _parity_range(
    rom: bytes,
    *,
    symbol: str,
    address: int,
    source_bytes: bytes,
    source_path: Path,
) -> dict[str, Any]:
    actual = rom[address : address + len(source_bytes)]
    if actual != source_bytes:
        mismatch = next(
            index
            for index, (expected, observed) in enumerate(zip(source_bytes, actual, strict=True))
            if expected != observed
        )
        raise ValueError(
            f"{symbol} source-ROM mismatch at +{mismatch}: "
            f"source={source_bytes[mismatch]}, ROM={actual[mismatch]}"
        )
    return {
        "symbol": symbol,
        "sourcePath": source_path.as_posix(),
        "address": address,
        "endExclusive": address + len(source_bytes),
        "byteCount": len(source_bytes),
        "sha256": hashlib.sha256(source_bytes).hexdigest().upper(),
    }


def _parse_shops(source: str, equates: dict[str, int]) -> tuple[list[dict[str, Any]], bytes]:
    rows = _arguments(source, "shopInventory")
    shops: list[dict[str, Any]] = []
    encoded = bytearray()
    for index, expression in enumerate(rows):
        codes = _tokens(expression)
        items = [_named_value(code, "ITEM_", equates) for code in codes]
        encoded.extend((len(items), *(item["value"] for item in items)))
        shops.append(
            {
                "index": index,
                "kind": "weapon" if index < 15 else "item",
                "items": items,
            }
        )
    if len(shops) != 30:
        raise ValueError(f"shop inventory count drift: {len(shops)}")
    return shops, bytes(encoded)


def _parse_mithril(
    source: str, equates: dict[str, int]
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], bytes, bytes]:
    groups: list[dict[str, Any]] = []
    class_bytes = bytearray()
    for index, expression in enumerate(_arguments(source, "classes")):
        codes = _tokens(expression)
        classes = [_named_value(code, "CLASS_", equates) for code in codes]
        class_bytes.extend(len(classes).to_bytes(2, "big"))
        for item in classes:
            class_bytes.extend(item["value"].to_bytes(2, "big"))
        groups.append({"index": index, "classes": classes})

    weapon_rows: list[dict[str, Any]] = []
    weapon_bytes = bytearray()
    for index, expression in enumerate(_arguments(source, "mithrilWeapons")):
        tokens = _tokens(expression)
        if len(tokens) != 8:
            raise ValueError(f"mithril row width drift at {index}: {len(tokens)}")
        remaining = Fraction(1, 1)
        choices: list[dict[str, Any]] = []
        for offset in range(0, len(tokens), 2):
            denominator = _integer(tokens[offset])
            item = _named_value(tokens[offset + 1], "ITEM_", equates)
            probability = remaining / denominator
            remaining -= probability
            choices.append(
                {
                    "denominator": denominator,
                    "item": item,
                    "probabilityNumerator": probability.numerator,
                    "probabilityDenominator": probability.denominator,
                }
            )
            weapon_bytes.extend((denominator, item["value"]))
        if remaining:
            raise ValueError(f"mithril row {index} does not end in a guaranteed choice")
        weapon_rows.append({"index": index, "choices": choices})
    if len(groups) != 9 or len(weapon_rows) != 8:
        raise ValueError("mithril class/weapon table shape drift")
    return groups, weapon_rows, bytes(class_bytes), bytes(weapon_bytes)


def _source_contract(disasm: Path, addresses: dict[str, int], rom: bytes) -> dict[str, Any]:
    equates = _parse_equates(disasm)
    sources = {
        name: read_upstream_text(disasm / path) for name, path in SOURCE_PATHS.items()
    }

    shops, shop_bytes = _parse_shops(sources["shops"], equates)

    debug_values = _byte_values(sources["debugShop"])
    if any(not isinstance(value, int) for value in debug_values):
        raise ValueError("debug-shop inventory contains unresolved values")
    debug_bytes = bytes(debug_values)
    if debug_bytes != bytes([128, *range(128)]):
        raise ValueError("debug-shop inventory no longer enumerates item IDs 0..127")

    chest_gold = [_integer(token) for token in _arguments(sources["chestGold"], "dc.w")]
    chest_bytes = b"".join(value.to_bytes(2, "big") for value in chest_gold)

    break_messages: list[dict[str, Any]] = []
    break_bytes = bytearray()
    for expression in _arguments(sources["breakMessages"], "itemBreakMessage"):
        item_code, message_code = _tokens(expression)
        item = _named_value(item_code, "ITEM_", equates)
        message = _named_value(message_code, "ITEMBREAK_", equates)
        break_messages.append({"item": item, "messageOffset": message})
        break_bytes.extend((item["value"], message["value"]))
    break_bytes.extend(b"\xFF\xFF")

    mithril_groups, mithril_rows, class_bytes, weapon_bytes = _parse_mithril(
        sources["mithril"], equates
    )

    special_descriptions: list[dict[str, Any]] = []
    special_bytes = bytearray()
    for expression in _arguments(sources["specialCaravan"], "specialCaravanDescription"):
        item_code, line_count, message_code = _tokens(expression)
        item = _named_value(item_code, "ITEM_", equates)
        message = _named_value(message_code, "MESSAGE_CARAVANDESC_", equates)
        special_descriptions.append(
            {"item": item, "lineCount": _integer(line_count), "firstMessage": message}
        )
        special_bytes.extend((item["value"], _integer(line_count)))
        special_bytes.extend(message["value"].to_bytes(2, "big"))
    special_bytes.extend(b"\xFF\xFF")

    field_codes = [
        token
        for expression in _arguments(sources["fieldItems"], "item")
        for token in _tokens(expression)
    ]
    field_items = [_named_value(code, "ITEM_", equates) for code in field_codes]
    field_bytes = bytes([*(item["value"] for item in field_items), 0xFF])

    weapon_graphics: list[dict[str, Any]] = []
    graphics_bytes = bytearray()
    weapon_start = equates["ITEMINDEX_WEAPONS_START"]
    for offset, expression in enumerate(_arguments(sources["weaponGraphics"], "weaponGraphics")):
        sprite_code, palette_code = _tokens(expression)
        sprite = _named_value(sprite_code, "WEAPONSPRITE_", equates)
        palette = _named_value(palette_code, "WEAPONPALETTE_", equates)
        weapon_graphics.append(
            {
                "itemIndex": weapon_start + offset,
                "sprite": sprite,
                "palette": palette,
            }
        )
        graphics_bytes.extend((sprite["value"], palette["value"]))

    ranges = [
        _parity_range(
            rom,
            symbol="list_ShopInventories",
            address=addresses["list_ShopInventories"],
            source_bytes=shop_bytes,
            source_path=SOURCE_PATHS["shops"],
        ),
        _parity_range(
            rom,
            symbol="list_DebugShopInventory",
            address=addresses["list_DebugShopInventory"],
            source_bytes=debug_bytes,
            source_path=SOURCE_PATHS["debugShop"],
        ),
        _parity_range(
            rom,
            symbol="table_ChestGoldAmounts",
            address=addresses["table_ChestGoldAmounts"],
            source_bytes=chest_bytes,
            source_path=SOURCE_PATHS["chestGold"],
        ),
        _parity_range(
            rom,
            symbol="table_ItemBreakMessages",
            address=addresses["table_ItemBreakMessages"],
            source_bytes=bytes(break_bytes),
            source_path=SOURCE_PATHS["breakMessages"],
        ),
        _parity_range(
            rom,
            symbol="list_MithrilWeaponClasses",
            address=addresses["list_MithrilWeaponClasses"],
            source_bytes=class_bytes,
            source_path=SOURCE_PATHS["mithril"],
        ),
        _parity_range(
            rom,
            symbol="table_MithrilWeapons",
            address=addresses["table_MithrilWeapons"],
            source_bytes=weapon_bytes,
            source_path=SOURCE_PATHS["mithril"],
        ),
        _parity_range(
            rom,
            symbol="table_SpecialCaravanDescriptions",
            address=addresses["table_SpecialCaravanDescriptions"],
            source_bytes=bytes(special_bytes),
            source_path=SOURCE_PATHS["specialCaravan"],
        ),
        _parity_range(
            rom,
            symbol="table_UsableOnFieldItems",
            address=addresses["table_UsableOnFieldItems"],
            source_bytes=field_bytes,
            source_path=SOURCE_PATHS["fieldItems"],
        ),
        _parity_range(
            rom,
            symbol="table_WeaponGraphics",
            address=addresses["table_WeaponGraphics"],
            source_bytes=bytes(graphics_bytes),
            source_path=SOURCE_PATHS["weaponGraphics"],
        ),
    ]

    consumers = {
        name: read_upstream_text(disasm / path) for name, path in CONSUMER_PATHS.items()
    }
    _assert_consumer(
        consumers["shops"],
        "shop inventory",
        (
            "lea     list_ShopInventories(pc), a0",
            "move.b  (CURRENT_SHOP_INDEX).l,d7",
            "subq.b  #1,d7",
            "move.b  (a0)+,d0",
            "adda.w  d0,a0",
        ),
    )
    _assert_consumer(
        consumers["mithril"],
        "mithril selection",
        (
            "move.w  #MITHRIL_WEAPON_CLASSES_COUNTER,d7",
            "move.w  #2,d6",
            "move.w  #2,d0",
            "lsl.w   #3,d0",
            "move.w  #MITHRIL_WEAPONS_PER_CLASS_COUNTER,d5",
            "jsr     (GenerateRandomNumber).w",
        ),
    )
    _assert_consumer(
        consumers["chestGold"],
        "chest gold",
        (
            "subi.w  #ITEMINDEX_GOLDCHESTS_START,d2",
            "andi.w  #ITEMENTRY_MASK_INDEX,d2",
            "move.w  table_ChestGoldAmounts(pc,d2.w),d1",
        ),
    )
    _assert_consumer(
        consumers["breakMessages"],
        "item-break message",
        (
            "lea     table_ItemBreakMessages(pc), a0",
            "cmpi.w  #-1,(a0)",
            "move.b  1(a0),d0",
            "add.w   d0,d3",
        ),
    )
    _assert_consumer(
        consumers["specialCaravan"],
        "special Caravan description",
        (
            "lea     table_SpecialCaravanDescriptions(pc), a0",
            "move.b  1(a0),d1",
            "move.w  2(a0),d0",
            "addq.w  #1,d0",
        ),
    )
    _assert_consumer(
        consumers["fieldItems"],
        "field-item allowlist",
        (
            "lea     table_UsableOnFieldItems(pc), a0",
            "cmp.b   (a0)+,d1",
            "cmpi.b  #-1,(a0)",
        ),
    )
    _assert_consumer(
        consumers["weaponGraphics"],
        "weapon graphics",
        (
            "cmpi.w  #ITEMINDEX_WEAPONS_START,d1",
            "cmpi.w  #ITEMINDEX_WEAPONS_END,d1",
            "subi.w  #ITEMINDEX_WEAPONS_START,d1",
            "move.b  (a0,d1.w),d2",
            "move.b  1(a0,d1.w),d3",
        ),
    )

    if chest_gold != list(range(10, 131, 10)):
        raise ValueError("chest gold tiers no longer form 10..130 by ten")
    if len(break_messages) != 25 or len(field_items) != 9 or len(weapon_graphics) != 84:
        raise ValueError("item auxiliary table count drift")
    if weapon_graphics[-1]["itemIndex"] != equates["ITEMINDEX_WEAPONS_END"]:
        raise ValueError("weapon graphics item range drift")

    summary = {
        "sourceFileCount": len(SOURCE_PATHS),
        "consumerFileCount": len(CONSUMER_PATHS),
        "romRangeCount": len(ranges),
        "sourceRomParityByteCount": sum(row["byteCount"] for row in ranges),
        "shopCount": len(shops),
        "weaponShopCount": sum(shop["kind"] == "weapon" for shop in shops),
        "itemShopCount": sum(shop["kind"] == "item" for shop in shops),
        "shopItemReferenceCount": sum(len(shop["items"]) for shop in shops),
        "uniqueShopInventoryCount": len(
            {tuple(item["value"] for item in shop["items"]) for shop in shops}
        ),
        "debugShopItemCount": len(debug_bytes) - 1,
        "chestGoldTierCount": len(chest_gold),
        "breakMessageCount": len(break_messages),
        "mithrilClassGroupCount": len(mithril_groups),
        "mithrilWeaponRowCount": len(mithril_rows),
        "mithrilChoiceCount": sum(len(row["choices"]) for row in mithril_rows),
        "specialCaravanDescriptionCount": len(special_descriptions),
        "fieldUsableItemCount": len(field_items),
        "weaponGraphicsCount": len(weapon_graphics),
        "weaponGraphicsNoneSpriteCount": sum(
            row["sprite"]["value"] == equates["WEAPONSPRITE_NONE"]
            for row in weapon_graphics
        ),
    }
    return {
        "summary": summary,
        "romRanges": ranges,
        "shops": shops,
        "debugShop": {
            "declaredCount": debug_bytes[0],
            "itemIds": list(debug_bytes[1:]),
        },
        "chestGold": chest_gold,
        "breakMessages": break_messages,
        "mithril": {
            "classGroups": mithril_groups,
            "weaponRows": mithril_rows,
            "specialClassGroup": {
                "index": 8,
                "classCodes": ["BRN", "RDBN"],
                "randomRowIndexes": [0, 2],
                "randomRange": 2,
            },
        },
        "specialCaravanDescriptions": special_descriptions,
        "fieldUsableItems": field_items,
        "weaponGraphics": weapon_graphics,
        "consumerRules": {
            "shops": {
                "indexBase": 0,
                "storage": "count byte followed by item bytes",
                "selection": "skip index count-prefixed records from list start",
            },
            "debugShop": "one count byte followed by every item index 0..127",
            "chestGold": "word[(itemIndex-128)&127] with no local bounds check",
            "breakMessages": "matched item byte adds its offset to the selected base message",
            "mithril": (
                "groups 0..7 select rows directly; BRN/RDBN randomly select row 0 or 2; "
                "each row tests denominators 16,8,4,1 in order"
            ),
            "specialCaravan": "matched entry displays lineCount consecutive messages",
            "fieldItems": "linear byte allowlist terminated by 255",
            "weaponGraphics": (
                "allies with equipped item 26..109 select two signed bytes by itemIndex-26; "
                "all other cases return -1/-1"
            ),
        },
        "runtimeQuestions": [
            "Which story and debug callers admit each of the 30 shop indexes, including the "
            "gaps in named SHOP_ITEM enums?",
            "Do blacksmith presentation, order persistence, and observed RNG frequencies match "
            "the statically derived row selection?",
        ],
    }


def build_item_auxiliary_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"item auxiliary H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom_identity = inspect_rom(rom_path)
    contract = _source_contract(disasm, addresses, rom_path.read_bytes())
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_identity["sha256"],
        **contract,
    }


def verify_item_auxiliary_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_item_auxiliary_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="item auxiliary static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("item auxiliary provenance drift")
    actual_table = {row["symbol"]: row["address"] for row in output["romRanges"]}
    for field, actual in (
        ("table", actual_table),
        ("summary", output["summary"]),
        ("consumerRules", output["consumerRules"]),
        ("runtimeQuestions", output["runtimeQuestions"]),
    ):
        if fixture[field] != actual:
            raise ValueError(f"item auxiliary fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if output["summary"] != manifest["summary"] or digest != manifest["outputSha256"]:
        raise ValueError("item auxiliary canonical manifest drift")
    destination = output_path or repo_path("local/derived/item-auxiliary-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Tables": output["summary"]["romRangeCount"],
        "Shops": output["summary"]["shopCount"],
        "ParityBytes": output["summary"]["sourceRomParityByteCount"],
        "Status": "PASS",
    }
