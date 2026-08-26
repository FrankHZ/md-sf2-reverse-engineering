from __future__ import annotations

import hashlib
import json
from copy import deepcopy

import pytest

import sf2tool.h2.field_item_effects as field_item_effects
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
ROM = repo_path("local/roms/sf2-us.bin")
FIXTURE = field_item_effects.FIXTURE
SCHEMA = field_item_effects.SCHEMA
INDEX = repo_path("manifests/research-index.json")
INDEX_SCHEMA = repo_path("schemas/research-index.schema.json")
_PRE_SLICE_INDEX_SHA256 = "5DB260172DBCA315A46AE1E713785BCE0E97081939500E2C213BB8232D1C5927"
_DIRECT_STATE_FIXTURE_ID = "sf2-map-event-direct-state-static-v1"
_DIRECT_STATE_VERIFIER = "src/sf2tool/h2/map_event_direct_state.py"
_DIRECT_STATE_DOCUMENT = "docs/research/map-event-direct-state.md"
_DIRECT_CONTROL_FIXTURE_ID = "sf2-map-event-direct-control-static-v1"
_DIRECT_CONTROL_DOCUMENT = "docs/research/map-event-direct-control.md"
_HANDOFF_FIXTURE_ID = "sf2-map-event-direct-handoff-static-v1"
_HANDOFF_DOCUMENT = "docs/research/map-event-direct-handoff.md"
_PREDICATE_FIXTURE_ID = "sf2-map-event-predicate-results-static-v1"
_PREDICATE_DOCUMENT = "docs/research/map-event-predicate-results.md"
_DIALOGUE_STATE_FIXTURE_ID = "sf2-map-event-dialogue-state-static-v1"
_DIALOGUE_STATE_DOCUMENT = "docs/research/map-event-dialogue-state.md"
_DIALOGUE_STATE_OWNER_IDS = {
    "map.data.ms-map3-flag506-entityevents",
    "map.data.ms-map3-zoneevents",
    "map.data.ms-map5-flag530-entityevents",
    "map.data.ms-map5-flag650-entityevents",
    "map.data.ms-map6-flag701-entityevents",
    "map.data.ms-map16-flag530-entityevents",
    "map.data.ms-map18-entityevents",
    "map.data.ms-map19-flag506-entityevents",
    "map.data.ms-map20-flag543-zoneevents",
    "map.data.ms-map21-flag506-entityevents",
    "map.data.ms-map25-entityevents",
    "map.data.ms-map37-section5",
    "map.data.ms-map40-entityevents",
    "map.data.ms-map44-flag507-entityevents",
    "map.data.ms-map63-entityevents",
    "map.data.ms-map72-zoneevents",
    "map.data.ms-map77-section5",
}
_DIRECT_STATE_OWNER_IDS = {
    "map.data.ms-map2-entityevents", "map.data.ms-map3-flag506-entityevents",
    "map.data.ms-map3-flag609-entityevents", "map.data.ms-map5-flag530-entityevents",
    "map.data.ms-map5-flag650-entityevents", "map.data.ms-map6-flag701-entityevents",
    "map.data.ms-map8-entityevents", "map.data.ms-map9-entityevents",
    "map.data.ms-map10-entityevents", "map.data.ms-map13-entityevents",
    "map.data.ms-map13-flag513-entityevents", "map.data.ms-map15-entityevents",
    "map.data.ms-map16-entityevents", "map.data.ms-map16-flag530-entityevents",
    "map.data.ms-map18-entityevents", "map.data.ms-map19-flag506-entityevents",
    "map.data.ms-map21-flag506-entityevents", "map.data.ms-map25-entityevents",
    "map.data.ms-map29-entityevents", "map.data.ms-map31-flag830-entityevents",
    "map.data.ms-map38-entityevents", "map.data.ms-map40-entityevents",
    "map.data.ms-map44-flag507-entityevents", "map.data.ms-map63-entityevents",
    "map.data.ms-map3-zoneevents", "map.data.ms-map16-zoneevents",
    "map.data.ms-map20-flag543-zoneevents", "map.data.ms-map22-zoneevents",
    "map.data.ms-map28-zoneevents", "map.data.ms-map66-zoneevents",
    "map.data.ms-map69-zoneevents", "map.data.ms-map70-zoneevents",
    "map.data.ms-map72-zoneevents", "map.data.ms-map74-zoneevents",
    "map.data.ms-map76-zoneevents", "map.data.ms-map77-zoneevents",
    "map.data.ms-map37-section5", "map.data.ms-map77-section5",
}


def _without_dialogue_state(index):
    normalized = deepcopy(index)
    removed: set[str] = set()
    for record in normalized["records"]:
        evidence = [
            item for item in record["evidence"] if item["fixtureId"] == _DIALOGUE_STATE_FIXTURE_ID
        ]
        if not evidence:
            continue
        assert record["id"] in _DIALOGUE_STATE_OWNER_IDS
        assert evidence == [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-dialogue-state-static-v1.json",
                "fixtureId": _DIALOGUE_STATE_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_dialogue_state.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            "eventDialogueState.sourceFiles."
                            f"{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
        assert record["documents"][-1] == _DIALOGUE_STATE_DOCUMENT
        record["evidence"].remove(evidence[0])
        record["documents"].pop()
        removed.add(record["id"])
    assert removed == _DIALOGUE_STATE_OWNER_IDS
    return normalized


def _without_predicate_results(index):
    normalized = deepcopy(index)
    removed: set[str] = set()
    for record in normalized["records"]:
        evidence = [
            item for item in record["evidence"] if item["fixtureId"] == _PREDICATE_FIXTURE_ID
        ]
        if not evidence:
            continue
        assert evidence == [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-predicate-results-static-v1.json",
                "fixtureId": _PREDICATE_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_predicate_results.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            "eventPredicateResults.sourceFiles."
                            f"{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
        assert record["documents"][-1] == _PREDICATE_DOCUMENT
        record["evidence"].remove(evidence[0])
        record["documents"].pop()
        removed.add(record["id"])
    assert len(removed) == 15
    return normalized


def _canonical_sha256(value: object) -> str:
    return (
        hashlib.sha256(json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8"))
        .hexdigest()
        .upper()
    )


def test_field_item_effects_fixture_is_canonical_and_recursively_closed() -> None:
    fixture = load_json(FIXTURE)
    assert set(fixture) == {
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "sourceContext",
        "retainedOwners",
        "fieldItemEffects",
        "unknowns",
        "summary",
    }
    assert set(fixture["fieldItemEffects"]) == {
        "callers",
        "usability",
        "dispatch",
        "randomGain",
        "effects",
    }
    assert set(fixture["fieldItemEffects"]["effects"]) == {
        "curePoison",
        "curePoisonAndParalysis",
        "increaseAtt",
        "increaseDef",
        "increaseAgi",
        "increaseMov",
        "increaseHp",
        "increaseMp",
        "levelUp",
        "dispatchOrder",
    }
    assert fixture["summary"] == {
        "sourceFiles": 8,
        "h1RomAnchors": 14,
        "callers": 2,
        "unknowns": 12,
    }
    assert fixture["unknowns"] == {key: "Unknown" for key in field_item_effects._UNKNOWN_KEYS}
    assert fixture["fieldItemEffects"]["effects"]["dispatchOrder"] == [
        {"itemId": 3, "effect": "curePoison"},
        {"itemId": 5, "effect": "curePoisonAndParalysis"},
        {"itemId": 9, "effect": "increaseAtt"},
        {"itemId": 10, "effect": "increaseDef"},
        {"itemId": 11, "effect": "increaseAgi"},
        {"itemId": 12, "effect": "increaseMov"},
        {"itemId": 13, "effect": "increaseHp"},
        {"itemId": 14, "effect": "increaseMp"},
        {"itemId": 15, "effect": "levelUp"},
    ]
    assert fixture["fieldItemEffects"]["effects"]["curePoisonAndParalysis"] == {
        "address": 141936,
        "statusBits": [1, 0],
        "statusMasks": [2, 1],
        "clearOperation": "bclr",
        "effectPresentBranch": "bne",
        "callOrder": ["GetStatusEffects", "SetStatusEffects", "UpdateCombatantStats"],
        "updateCombatantStatsAddress": 35278,
        "textIds": [149, 156, 148],
    }
    assert FIXTURE.read_bytes() == field_item_effects.canonical_json_bytes(fixture)
    validate_json(fixture, SCHEMA, owner="Field item effects fixture")
    field_item_effects._validate_structural_output(fixture)

    def assert_closed(value: object) -> None:
        if isinstance(value, dict):
            if value.get("type") == "object":
                assert value.get("additionalProperties") is False
                assert set(value["required"]).issubset(value["properties"])
            for child in value.values():
                assert_closed(child)
        elif isinstance(value, list):
            for child in value:
                assert_closed(child)

    assert_closed(load_json(SCHEMA))
    public = FIXTURE.read_text(encoding="utf-8").lower()
    for forbidden in (
        "raw source",
        "rom bytes",
        "h1 bytes",
        "runtime state",
        "local/",
        "capture",
        "movie",
        "emulator",
        "lua",
    ):
        assert forbidden not in public


def test_field_item_effects_schema_rejects_exact_shape_and_order_drift() -> None:
    fixture = load_json(FIXTURE)

    def rejects(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken)
        with pytest.raises(ValueError, match="schema"):
            validate_json(broken, SCHEMA, owner="Field item effects fixture")

    rejects(lambda value: value.__setitem__("unexpected", True))
    rejects(lambda value: value["sourceContext"].__setitem__("fieldItemEffects", {}))
    rejects(lambda value: value["fieldItemEffects"].pop("randomGain"))
    rejects(
        lambda value: value["fieldItemEffects"]["effects"]["increaseMov"].__setitem__("extra", 1)
    )
    rejects(
        lambda value: value["fieldItemEffects"]["effects"]["curePoisonAndParalysis"].__setitem__(
            "noUseBranch", "bne"
        )
    )
    rejects(lambda value: value["unknowns"].pop("actual-random-gain"))
    rejects(lambda value: value["fieldItemEffects"]["usability"].__setitem__("itemIds", [3, 5]))
    rejects(
        lambda value: value["fieldItemEffects"]["effects"].__setitem__(
            "dispatchOrder", list(reversed(value["fieldItemEffects"]["effects"]["dispatchOrder"]))
        )
    )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_field_item_effects_source_contract_matches_public_fixture() -> None:
    source, identities = field_item_effects._read_source_surface(UPSTREAM / "disasm")
    parsed = field_item_effects._validate_source_contract(source)
    fixture = load_json(FIXTURE)
    assert len(identities) == 8
    assert parsed["sourceContext"] == {
        key: fixture["sourceContext"][key]
        for key in ("layoutCanonicalIncludes", "excludedAlternates", "callerInventory")
    }
    assert parsed["fieldItemEffects"] == fixture["fieldItemEffects"]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("path", "needle", "replacement"),
    (
        (
            "code/common/menus/main/mainactions.asm",
            "bsr.w   UseItemOnField",
            "bsr.w   UseItemOnMenu",
        ),
        (
            "code/common/menus/caravan/caravanactions_1.asm",
            "bsr.w   UseItemOnField",
            "bsr.w   UseItemOnMenu",
        ),
        ("code/common/menus/item/isitemusableonfield.asm", "beq.w   @Return", "bne.w   @Return"),
        ("data/stats/items/usableoutsidebattleitems.asm", "item BRAVE_APPLE", "item ANGEL_WING"),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "andi.w  #ITEMENTRY_MASK_INDEX,d1",
            "andi.w  #ITEMENTRY_MASK_INDEX_AND_BROKEN_BIT,d1",
        ),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "bclr    #STATUSEFFECT_BIT_POISON,d1",
            "bset    #STATUSEFFECT_BIT_POISON,d1",
        ),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "bclr    #STATUSEFFECT_BIT_STUN,d1",
            "bset    #STATUSEFFECT_BIT_STUN,d1",
        ),
        ("code/common/menus/item/fielditemeffects.asm", "moveq   #3,d6", "moveq   #2,d6"),
        ("code/common/menus/item/fielditemeffects.asm", "addq.w  #2,d7", "addq.w  #3,d7"),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "jsr     j_IncreaseCurrentAgi",
            "jsr     j_IncreaseBaseAgi",
        ),
        ("code/common/menus/item/fielditemeffects.asm", "cmpi.b  #9,d1", "cmpi.b  #7,d1"),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "jsr     j_IncreaseMaxHp",
            "jsr     j_IncreaseCurrentHp",
        ),
        ("code/common/menus/item/fielditemeffects.asm", "beq.s   byte_22BBC", "bne.s   byte_22BBC"),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "jsr     j_LevelUp",
            "jsr     j_SetCurrentExp",
        ),
    ),
)
def test_field_item_effects_source_and_anchor_mutations_fail_before_fixture_comparison(
    path: str, needle: str, replacement: str
) -> None:
    source, _ = field_item_effects._read_source_surface(UPSTREAM / "disasm")
    mutated = dict(source)
    assert needle in mutated[path]
    mutated[path] = mutated[path].replace(needle, replacement, 1)
    with pytest.raises(ValueError, match="Field item effects"):
        field_item_effects._validate_source_contract(mutated)


_TEXT_USE_SITES = (
    (149, 1),
    (148, 1),
    (149, 2),
    (156, 1),
    (148, 2),
    (150, 1),
    (151, 1),
    (152, 1),
    (153, 1),
    (154, 1),
    (155, 1),
    (148, 3),
    (148, 4),
    (244, 1),
    (266, 1),
    (267, 1),
    (268, 1),
    (269, 1),
    (270, 1),
    (271, 1),
    (272, 1),
    (3523, 1),
)


def _replace_text_use_site(source: str, text_id: int, occurrence: int) -> str:
    marker = f"txt     {text_id}"
    start = -1
    for _ in range(occurrence):
        start = source.index(marker, start + 1)
    return source[:start] + source[start:].replace(marker, "txt     9999", 1)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(("text_id", "occurrence"), _TEXT_USE_SITES)
def test_field_item_effects_every_public_text_use_site_mutation_fails(
    text_id: int, occurrence: int
) -> None:
    source, _ = field_item_effects._read_source_surface(UPSTREAM / "disasm")
    mutated = dict(source)
    path = "code/common/menus/item/fielditemeffects.asm"
    mutated[path] = _replace_text_use_site(mutated[path], text_id, occurrence)
    with pytest.raises(ValueError, match="Field item effects"):
        field_item_effects._validate_source_contract(mutated)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("path", "needle", "replacement"),
    (
        (
            "data/stats/items/usableoutsidebattleitems.asm",
            "tableEnd.b",
            "tableEnd.w",
        ),
        (
            "data/stats/items/usableoutsidebattleitems.asm",
            "tableEnd.b",
            "",
        ),
        (
            "data/stats/items/usableoutsidebattleitems.asm",
            "tableEnd.b",
            "tableEnd.b\n                tableEnd.b",
        ),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "fieldItem_CurePoison-rjt_FieldItemEffects",
            "fieldItem_CurePoison-rjt_FieldItemEffects+2",
        ),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "fieldItem_CurePoison-rjt_FieldItemEffects",
            "fieldItem_Broken-rjt_FieldItemEffects",
        ),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "dc.w fieldItem_CurePoison-rjt_FieldItemEffects",
            "; missing dispatch offset",
        ),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "fieldItem_CurePoison-rjt_FieldItemEffects",
            "fieldItem_CurePoisonAndParalysis-rjt_FieldItemEffects",
        ),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "dc.w $FFFF",
            "dc.w $FFFE",
        ),
        (
            "code/common/menus/item/fielditemeffects.asm",
            "dc.w $FFFF",
            "dc.w $FFFF\n                dc.w $FFFF",
        ),
    ),
)
def test_field_item_effects_table_and_dispatch_structure_mutations_fail(
    path: str, needle: str, replacement: str
) -> None:
    source, _ = field_item_effects._read_source_surface(UPSTREAM / "disasm")
    mutated = dict(source)
    assert needle in mutated[path]
    mutated[path] = mutated[path].replace(needle, replacement, 1)
    with pytest.raises(ValueError, match="Field item effects"):
        field_item_effects._validate_source_contract(mutated)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_field_item_effects_swapped_dispatch_offsets_fail() -> None:
    source, _ = field_item_effects._read_source_surface(UPSTREAM / "disasm")
    mutated = dict(source)
    path = "code/common/menus/item/fielditemeffects.asm"
    first = "fieldItem_CurePoison-rjt_FieldItemEffects"
    second = "fieldItem_CurePoisonAndParalysis-rjt_FieldItemEffects"
    mutated[path] = (
        mutated[path]
        .replace(first, "fieldItem_Temporary-rjt_FieldItemEffects", 1)
        .replace(second, first, 1)
        .replace("fieldItem_Temporary-rjt_FieldItemEffects", second, 1)
    )
    with pytest.raises(ValueError, match="Field item effects"):
        field_item_effects._validate_source_contract(mutated)


def test_field_item_effect_call_parser_ignores_comments_labels_and_near_misses() -> None:
    source = """label: bsr.w UseItemOnField
; bsr.w UseItemOnField
bsr.s UseItemOnField
bsr.w UseItemOnField trailing
bsr.w UseItemOnField
"""
    assert field_item_effects._direct_calls(source, "UseItemOnField") == ["UseItemOnField"]
    assert field_item_effects._caller_paths_from_source_map(
        {
            "code/a.asm": source,
            "code/b.asm": "; bsr.w UseItemOnField\nbsr.s UseItemOnField\n",
            "code/c.asm": "bsr.w UseItemOnField\n",
        }
    ) == ["code/a.asm", "code/c.asm"]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_field_item_effects_complete_code_caller_scan_is_exact() -> None:
    assert field_item_effects._complete_code_caller_paths(UPSTREAM / "disasm") == [
        "code/common/menus/caravan/caravanactions_1.asm",
        "code/common/menus/main/mainactions.asm",
    ]


@pytest.mark.skipif(not ROM.is_file(), reason="canonical ROM is unavailable")
def test_field_item_effects_every_h1_rom_anchor_rejects_one_byte_drift() -> None:
    rom = ROM.read_bytes()
    assert (
        field_item_effects._anchor_projection(rom, rom)
        == load_json(FIXTURE)["sourceContext"]["h1RomAnchors"]
    )
    for _, address, _ in field_item_effects._ANCHORS:
        drifted = bytearray(rom)
        drifted[address] ^= 1
        with pytest.raises(ValueError, match="H1/ROM anchor drift"):
            field_item_effects._anchor_projection(bytes(drifted), rom)


def test_field_item_effects_index_delta_is_exact_and_rejects_unknown_roots() -> None:
    index = _without_predicate_results(_without_dialogue_state(load_json(INDEX)))
    records = {record["id"]: record for record in index["records"]}
    expected = {
        (
            "menus.field-main",
            "field-item-use-call",
            "fieldItemEffects.callers.fieldMenu.callAddress",
        ),
        (
            "menus.caravan-actions",
            "field-item-use-call",
            "fieldItemEffects.callers.caravan.callAddress",
        ),
        ("menus.field-item-usability", "entry", "fieldItemEffects.usability.functionAddress"),
        ("stats.data.field-items", "entry", "fieldItemEffects.usability.tableAddress"),
        ("menus.field-item-effects", "entry", "fieldItemEffects.dispatch.functionAddress"),
        ("menus.field-item-effects", "dispatch-table", "fieldItemEffects.dispatch.tableAddress"),
        ("menus.field-item-effects", "cure-poison", "fieldItemEffects.effects.curePoison.address"),
        (
            "menus.field-item-effects",
            "cure-poison-paralysis",
            "fieldItemEffects.effects.curePoisonAndParalysis.address",
        ),
        (
            "menus.field-item-effects",
            "increase-att",
            "fieldItemEffects.effects.increaseAtt.address",
        ),
        (
            "menus.field-item-effects",
            "increase-def",
            "fieldItemEffects.effects.increaseDef.address",
        ),
        (
            "menus.field-item-effects",
            "increase-agi",
            "fieldItemEffects.effects.increaseAgi.address",
        ),
        (
            "menus.field-item-effects",
            "increase-mov",
            "fieldItemEffects.effects.increaseMov.address",
        ),
        ("menus.field-item-effects", "increase-hp", "fieldItemEffects.effects.increaseHp.address"),
        ("menus.field-item-effects", "increase-mp", "fieldItemEffects.effects.increaseMp.address"),
        ("menus.field-item-effects", "level-up", "fieldItemEffects.effects.levelUp.address"),
        ("rng.generate-random-number", "entry", "fieldItemEffects.randomGain.generatorAddress"),
        ("growth.level-up", "entry", "fieldItemEffects.effects.levelUp.levelUpAddress"),
        (
            "growth.level-up",
            "level-up-arguments",
            "fieldItemEffects.effects.levelUp.argumentsAddress",
        ),
        (
            "growth.update-combatant-stats",
            "entry",
            "fieldItemEffects.effects.curePoisonAndParalysis.updateCombatantStatsAddress",
        ),
        (
            "battle.combatant.get-status-effects",
            "entry",
            "fieldItemEffects.effects.curePoison.getStatusEffectsAddress",
        ),
    }
    actual = {
        (record_id, binding["addressId"], binding["fixtureField"])
        for record_id, record in records.items()
        for evidence in record["evidence"]
        if evidence["fixtureId"] == field_item_effects.ID
        for binding in evidence["bindings"]
    }
    assert actual == expected
    assert len(actual) == 20
    expected_owner_ids = {
        "menus.field-main",
        "menus.caravan-actions",
        "menus.field-item-usability",
        "stats.data.field-items",
        "menus.field-item-effects",
        "rng.generate-random-number",
        "growth.level-up",
        "growth.update-combatant-stats",
        "battle.combatant.get-status-effects",
    }
    assert {record_id for record_id, _, _ in actual} == expected_owner_ids
    expected_new_addresses = {
        ("menus.field-main", "field-item-use-call", 136572),
        ("menus.caravan-actions", "field-item-use-call", 140752),
        ("menus.field-item-effects", "dispatch-table", 141858),
        ("menus.field-item-effects", "cure-poison", 141902),
        ("menus.field-item-effects", "cure-poison-paralysis", 141936),
        ("menus.field-item-effects", "increase-att", 141998),
        ("menus.field-item-effects", "increase-def", 142038),
        ("menus.field-item-effects", "increase-agi", 142078),
        ("menus.field-item-effects", "increase-mov", 142118),
        ("menus.field-item-effects", "increase-hp", 142178),
        ("menus.field-item-effects", "increase-mp", 142218),
        ("menus.field-item-effects", "level-up", 142274),
    }
    field_search_id = "sf2-field-search-control-static-v1"
    field_search_document = "docs/research/field-search-control.md"
    expected_later_bindings = {
        (
            "tech.interfaces.jump-s02",
            "increase-gold",
            "fieldSearchSpine.functionAddresses.j_IncreaseGold",
        ),
        (
            "tech.interfaces.jump-s02",
            "get-item-slots",
            "fieldSearchSpine.functionAddresses.j_GetItemBySlotAndHeldItemsNumber",
        ),
        ("tech.interfaces.jump-s02", "add-item", "fieldSearchSpine.functionAddresses.j_AddItem"),
        (
            "tech.interfaces.jump-s02",
            "update-force",
            "fieldSearchSpine.functionAddresses.j_UpdateForce",
        ),
        (
            "tech.interfaces.jump-s05",
            "check-area",
            "fieldSearchSpine.functionAddresses.j_CheckArea",
        ),
        (
            "tech.interfaces.jump-s07",
            "run-area-description",
            "fieldSearchSpine.functionAddresses.j_RunMapSetupAreaDescription",
        ),
        ("menus.field-main", "search-call", "fieldSearchSpine.callers.fieldMenu.callAddress"),
        (
            "gameflow.exploration.actions",
            "check-area-call",
            "fieldSearchSpine.callers.processPlayerActionNoEntity.callAddress",
        ),
        (
            "gameflow.exploration.interaction",
            "check-area",
            "fieldSearchSpine.functionAddresses.CheckArea",
        ),
        (
            "gameflow.exploration.interaction",
            "get-chest-gold",
            "fieldSearchSpine.functionAddresses.GetChestGoldAmount",
        ),
        (
            "gameflow.exploration.item-handoff",
            "entry",
            "fieldSearchSpine.functionAddresses.itemHandoff",
        ),
        (
            "gameflow.exploration.engine",
            "open-chest",
            "fieldSearchSpine.functionAddresses.OpenChest",
        ),
        (
            "gameflow.exploration.engine",
            "close-chest",
            "fieldSearchSpine.functionAddresses.CloseChest",
        ),
        (
            "gameflow.exploration.engine",
            "check-nonchest-item",
            "fieldSearchSpine.functionAddresses.CheckNonChestItem",
        ),
        (
            "gameflow.exploration.engine",
            "refill-nonchest-item",
            "fieldSearchSpine.functionAddresses.RefillNonChestItem",
        ),
        (
            "map.setup.area-description",
            "entry",
            "fieldSearchSpine.functionAddresses.RunMapSetupAreaDescription",
        ),
        ("stats.party", "entry", "fieldSearchSpine.functionAddresses.UpdateForce"),
        ("battle.replay.increase-gold", "entry", "fieldSearchSpine.functionAddresses.IncreaseGold"),
        (
            "stats.item-stats",
            "get-item-slots",
            "fieldSearchSpine.functionAddresses.GetItemBySlotAndHeldItemsNumber",
        ),
        ("stats.item-stats", "add-item", "fieldSearchSpine.functionAddresses.AddItem"),
        ("stats.data.chest-gold", "entry", "fieldSearchSpine.goldPath.tableAddress"),
    }
    later_owner_ids = {record_id for record_id, _, _ in expected_later_bindings}
    assert len(expected_later_bindings) == 21
    assert len(later_owner_ids) == 13
    expected_later_addresses = {
        ("tech.interfaces.jump-s02", "increase-gold", 33116),
        ("tech.interfaces.jump-s02", "get-item-slots", 33140),
        ("tech.interfaces.jump-s02", "add-item", 33176),
        ("tech.interfaces.jump-s02", "update-force", 33392),
        ("tech.interfaces.jump-s05", "check-area", 131148),
        ("tech.interfaces.jump-s07", "run-area-description", 278708),
        ("menus.field-main", "search-call", 137694),
        ("gameflow.exploration.actions", "check-area-call", 154562),
        ("gameflow.exploration.interaction", "get-chest-gold", 145820),
        ("gameflow.exploration.engine", "open-chest", 16726),
        ("gameflow.exploration.engine", "close-chest", 16788),
        ("gameflow.exploration.engine", "check-nonchest-item", 16886),
        ("gameflow.exploration.engine", "refill-nonchest-item", 16922),
        ("stats.item-stats", "get-item-slots", 35834),
        ("stats.item-stats", "add-item", 36002),
    }
    assert len(expected_later_addresses) == 15
    actual_later_bindings = {
        (record_id, binding["addressId"], binding["fixtureField"])
        for record_id, record in records.items()
        for evidence in record["evidence"]
        if evidence["fixtureId"] == field_search_id
        for binding in evidence["bindings"]
    }
    assert actual_later_bindings == expected_later_bindings
    assert {
        record_id
        for record_id, record in records.items()
        if field_search_document in record["documents"]
    } == later_owner_ids
    assert all(
        records[record_id]["documents"].count(field_search_document) == 1
        for record_id in later_owner_ids
    )
    assert records["menus.field-main"]["documents"][-2:] == [
        field_search_document,
        "docs/research/field-item-effects.md",
    ]
    actual_addresses = {
        (record_id, address["id"], address["value"])
        for record_id, record in records.items()
        for address in record["addresses"]
        if (record_id, address["id"]) in {(item[0], item[1]) for item in expected_new_addresses}
    }
    assert actual_addresses == expected_new_addresses
    assert all(
        records[record_id]["documents"][-1] == "docs/research/field-item-effects.md"
        for record_id, _, _ in actual
    )

    normalized = deepcopy(index)
    removed_evidence_records: set[str] = set()
    removed_addresses: set[tuple[str, str, int]] = set()
    removed_document_records: set[str] = set()
    removed_later_evidence_records: set[str] = set()
    removed_later_addresses: set[tuple[str, str, int]] = set()
    removed_later_document_records: set[str] = set()
    removed_direct_state_evidence_records: set[str] = set()
    removed_direct_state_document_records: set[str] = set()
    removed_direct_control_records: set[str] = set()
    removed_handoff_records: set[str] = set()

    def remove_later_field_search_document(documents: list[str]) -> None:
        assert documents[-2:] == [
            field_search_document,
            "docs/research/field-item-effects.md",
        ]
        assert documents.count(field_search_document) == 1
        documents.remove(field_search_document)

    for record in normalized["records"]:
        record_id = record["id"]
        handoff_evidence = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] == _HANDOFF_FIXTURE_ID
        ]
        if handoff_evidence:
            assert handoff_evidence == [
                {
                    "level": "H2",
                    "fixture": "tests/fixtures/h2/map-event-direct-handoff-static-v1.json",
                    "fixtureId": _HANDOFF_FIXTURE_ID,
                    "verifier": "src/sf2tool/h2/map_event_direct_handoff.py",
                    "bindings": [
                        {
                            "addressId": "entry",
                            "fixtureField": (
                                f"eventDirectHandoff.sourceFiles.{record['symbol']}.tableEntryAddress"
                            ),
                        }
                    ],
                }
            ]
            assert record["documents"].count(_HANDOFF_DOCUMENT) == 1
            assert record["documents"][-1] == _HANDOFF_DOCUMENT
            record["evidence"] = [
                evidence
                for evidence in record["evidence"]
                if evidence["fixtureId"] != _HANDOFF_FIXTURE_ID
            ]
            record["documents"].remove(_HANDOFF_DOCUMENT)
            removed_handoff_records.add(record_id)
        direct_control_evidence = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] == _DIRECT_CONTROL_FIXTURE_ID
        ]
        if direct_control_evidence:
            assert len(direct_control_evidence) == 1
            assert direct_control_evidence[0] == {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-direct-control-static-v1.json",
                "fixtureId": _DIRECT_CONTROL_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_direct_control.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            f"eventDirectControl.sourceFiles.{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
            assert record["documents"].count(_DIRECT_CONTROL_DOCUMENT) == 1
            assert record["documents"][-1] == _DIRECT_CONTROL_DOCUMENT
            record["evidence"] = [
                evidence
                for evidence in record["evidence"]
                if evidence["fixtureId"] != _DIRECT_CONTROL_FIXTURE_ID
            ]
            record["documents"].remove(_DIRECT_CONTROL_DOCUMENT)
            removed_direct_control_records.add(record_id)
        direct_state_evidence = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] == _DIRECT_STATE_FIXTURE_ID
        ]
        if direct_state_evidence:
            assert record_id in _DIRECT_STATE_OWNER_IDS
            assert len(direct_state_evidence) == 1
            assert direct_state_evidence[0]["level"] == "H2"
            assert direct_state_evidence[0]["fixture"] == (
                "tests/fixtures/h2/map-event-direct-state-static-v1.json"
            )
            assert direct_state_evidence[0]["verifier"] == _DIRECT_STATE_VERIFIER
            assert direct_state_evidence[0]["bindings"] == [
                {
                    "addressId": "entry",
                    "fixtureField": (
                        "eventDirectState.sourceFiles."
                        f"{record['symbol']}.tableEntryAddress"
                    ),
                }
            ]
            removed_direct_state_evidence_records.add(record_id)
        record["evidence"] = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] != _DIRECT_STATE_FIXTURE_ID
        ]
        field_item_evidence = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] == field_item_effects.ID
        ]
        if field_item_evidence:
            assert record_id in expected_owner_ids
            assert len(field_item_evidence) == 1
            assert field_item_evidence[0]["level"] == "H2"
            removed_evidence_records.add(record_id)
        record["evidence"] = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] != field_item_effects.ID
        ]
        later_evidence = [
            evidence for evidence in record["evidence"] if evidence["fixtureId"] == field_search_id
        ]
        if later_evidence:
            assert record_id in later_owner_ids
            assert len(later_evidence) == 1
            assert later_evidence[0]["level"] == "H2"
            assert later_evidence[0]["fixture"] == (
                "tests/fixtures/h2/field-search-control-static-v1.json"
            )
            assert later_evidence[0]["verifier"] == "src/sf2tool/h2/field_search_control.py"
            expected_record_bindings = {
                (address_id, fixture_field)
                for owner_id, address_id, fixture_field in expected_later_bindings
                if owner_id == record_id
            }
            actual_record_bindings = {
                (binding["addressId"], binding["fixtureField"])
                for binding in later_evidence[0]["bindings"]
            }
            assert actual_record_bindings == expected_record_bindings
            assert len(later_evidence[0]["bindings"]) == len(expected_record_bindings)
            removed_later_evidence_records.add(record_id)
        record["evidence"] = [
            evidence for evidence in record["evidence"] if evidence["fixtureId"] != field_search_id
        ]
        retained_addresses = []
        for address in record["addresses"]:
            address_key = (record_id, address["id"], address["value"])
            if address_key in expected_new_addresses:
                removed_addresses.add(address_key)
            elif address_key in expected_later_addresses:
                removed_later_addresses.add(address_key)
            else:
                retained_addresses.append(address)
        record["addresses"] = retained_addresses
        if _DIRECT_STATE_DOCUMENT in record["documents"]:
            assert record_id in _DIRECT_STATE_OWNER_IDS
            assert record["documents"].count(_DIRECT_STATE_DOCUMENT) == 1
            assert record["documents"][-1] == _DIRECT_STATE_DOCUMENT
            record["documents"].remove(_DIRECT_STATE_DOCUMENT)
            removed_direct_state_document_records.add(record_id)
        if field_search_document in record["documents"]:
            assert record_id in later_owner_ids
            assert record["documents"].count(field_search_document) == 1
            if record_id == "menus.field-main":
                remove_later_field_search_document(record["documents"])
            else:
                record["documents"].remove(field_search_document)
            removed_later_document_records.add(record_id)
        if "docs/research/field-item-effects.md" in record["documents"]:
            assert record_id in expected_owner_ids
            assert record["documents"][-1] == "docs/research/field-item-effects.md"
            record["documents"].remove("docs/research/field-item-effects.md")
            removed_document_records.add(record_id)

    assert removed_evidence_records == expected_owner_ids
    assert removed_addresses == expected_new_addresses
    assert removed_document_records == expected_owner_ids
    assert removed_later_evidence_records == later_owner_ids
    assert removed_later_addresses == expected_later_addresses
    assert removed_later_document_records == later_owner_ids
    assert removed_direct_state_evidence_records == _DIRECT_STATE_OWNER_IDS
    assert removed_direct_state_document_records == _DIRECT_STATE_OWNER_IDS
    assert len(removed_direct_control_records) == 53
    assert len(removed_handoff_records) == 53
    assert _canonical_sha256(normalized) == _PRE_SLICE_INDEX_SHA256

    malformed_later_documents = list(records["menus.field-main"]["documents"])
    malformed_later_documents[-2] = "docs/research/unrelated.md"
    with pytest.raises(AssertionError):
        remove_later_field_search_document(malformed_later_documents)

    def invalid(field: str) -> None:
        broken = deepcopy(index)
        binding = next(
            binding
            for record in broken["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == field_item_effects.ID
            for binding in evidence["bindings"]
        )
        binding["fixtureField"] = field
        with pytest.raises(ValueError, match="schema"):
            validate_json(broken, INDEX_SCHEMA, owner="field item effects index")

    invalid("unknownRoot.fieldItemEffects")
    invalid("sourceContext.fieldItemEffects.dispatch")


@pytest.mark.skipif(
    not (UPSTREAM / "build/sf2build-h1.bin").is_file(),
    reason="pinned H1 artifact is unavailable",
)
def test_field_item_effects_complete_verifier_matches_fixture() -> None:
    assert field_item_effects.verify_field_item_effects_static(ROM, UPSTREAM) == load_json(FIXTURE)
