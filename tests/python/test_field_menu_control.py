from __future__ import annotations

from copy import deepcopy

import pytest

import sf2tool.h2.field_menu_control as field_menu
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
FIXTURE = field_menu.FIXTURE
SCHEMA = field_menu.SCHEMA
INDEX = repo_path("manifests/research-index.json")
INDEX_SCHEMA = repo_path("schemas/research-index.schema.json")
ROM = repo_path("local/roms/sf2-us.bin")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_field_menu_source_spine_matches_public_fixture() -> None:
    source, identities = field_menu._read_source_surface(UPSTREAM / "disasm")
    fixture = load_json(FIXTURE)
    parsed = field_menu._validate_source_contract(
        source,
        usable_field_items_address=fixture["fieldMenuSpine"]["itemAction"]["use"][
            "usableFieldItemsAddress"
        ],
    )

    assert len(identities) == 9
    assert parsed["sourceContext"]["mainactionsShape"] == {
        "physicalLines": 696,
        "nonEmptyLines": 679,
        "statements": 481,
        "globalLabels": 12,
        "localLabels": 46,
        "directCalls": 69,
        "directCallTargets": 33,
    }
    assert parsed["fieldMenuSpine"] == fixture["fieldMenuSpine"]


def test_field_menu_fixture_is_canonical_and_schema_is_recursively_closed() -> None:
    fixture = load_json(FIXTURE)
    assert set(fixture) == {
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "system",
        "summary",
        "retainedOwners",
        "sourceContext",
        "fieldMenuSpine",
        "unknowns",
    }
    assert set(fixture["fieldMenuSpine"]) == {
        "functionAddresses",
        "jumpInterfaces",
        "entryAndCallers",
        "mainDispatch",
        "memberAction",
        "magicAction",
        "itemAction",
        "searchAction",
        "forceListHelper",
        "excludedUnusedTail",
    }
    assert fixture["summary"] == {"sourceFiles": 9, "h1RomAnchors": 23, "unknowns": 18}
    assert fixture["unknowns"] == {key: "Unknown" for key in field_menu._UNKNOWN_KEYS}
    assert FIXTURE.read_bytes() == field_menu.canonical_json_bytes(fixture)
    validate_json(fixture, SCHEMA, owner="FieldMenu fixture")
    field_menu._validate_structural_output(fixture)

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
    public = field_menu.canonical_json_bytes(fixture).decode("utf-8").lower()
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


def test_field_menu_fixture_schema_rejects_closed_shape_drift() -> None:
    fixture = load_json(FIXTURE)

    def rejects(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken)
        with pytest.raises(ValueError, match="schema"):
            validate_json(broken, SCHEMA, owner="FieldMenu fixture")

    rejects(lambda value: value.__setitem__("unexpected", True))
    rejects(lambda value: value["sourceContext"].__setitem__("fieldMenuSpine", {}))
    rejects(lambda value: value["fieldMenuSpine"].pop("magicAction"))
    rejects(lambda value: value["fieldMenuSpine"]["itemAction"]["drop"].__setitem__("extra", 1))
    rejects(lambda value: value["unknowns"].pop("actual-search-area-result"))
    rejects(
        lambda value: value["fieldMenuSpine"]["magicAction"]["egress"].__setitem__(
            "callOrder", ["DecreaseCurrentMp", "GetSpellDefinitionAddress"]
        )
    )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("needle", "replacement"),
    (
        (
            "cmpi.w  #2,d0\n                bne.w   @SearchAction",
            "cmpi.w  #3,d0\n                bne.w   @SearchAction",
        ),
        ("beq.s   @ExitMain", "bne.s   @ExitMain"),
        ("cmpi.w  #SPELL_EGRESS,spellIndex(a6)", "cmpi.w  #SPELL_DETOX,spellIndex(a6)"),
        (
            "jsr     j_DecreaseCurrentMp\n                jsr     j_ExecuteFlashScreenScript",
            "jsr     j_ExecuteFlashScreenScript\n                jsr     j_DecreaseCurrentMp",
        ),
        ("move.b  d3,(a0)+", "move.b  d4,(a0)+"),
        (
            "cmpi.w  #2,d0\n                bne.w   @ItemDropAction",
            "cmpi.w  #3,d0\n                bne.w   @ItemDropAction",
        ),
        ("andi.b  #ITEMTYPE_UNSELLABLE,d1", "andi.b  #ITEMTYPE_RARE,d1"),
        ("beq.s   byte_219D0", "bne.s   byte_219D0"),
        ("jsr     j_CheckArea", "jsr     j_UpdateForce"),
        ("dbf     d7,@Copy_Loop", "dbf     d6,@Copy_Loop"),
        (
            'txt     312             ; "But nothing happened."',
            'txt     313             ; "But nothing happened."',
        ),
        (
            'txt     108             ; "Use magic on whom?{D1}"',
            'txt     109             ; "Use magic on whom?{D1}"',
        ),
        (
            'txt     73              ; "{NAME} used the{N}{ITEM}.{W2}"',
            'txt     75              ; "{NAME} used the{N}{ITEM}.{W2}"',
        ),
        (
            'txt     54              ; "Pass the {ITEM}{N}to whom?{D1}"',
            'txt     56              ; "Pass the {ITEM}{N}to whom?{D1}"',
        ),
        (
            'txt     62              ; "{LEADER}!  You can\'t{N}discard the {ITEM}!{W2}"',
            'txt     63              ; "{LEADER}!  You can\'t{N}discard the {ITEM}!{W2}"',
        ),
    ),
)
def test_field_menu_source_mutations_fail_before_fixture_comparison(
    needle: str, replacement: str
) -> None:
    source, _ = field_menu._read_source_surface(UPSTREAM / "disasm")
    mutated = dict(source)
    main = mutated["code/common/menus/main/mainactions.asm"]
    assert needle in main
    mutated["code/common/menus/main/mainactions.asm"] = main.replace(needle, replacement, 1)
    with pytest.raises(ValueError, match="FieldMenu"):
        field_menu._validate_source_contract(mutated, usable_field_items_address=141794)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_field_menu_caller_inventory_rejects_alias_target_drift() -> None:
    source, _ = field_menu._read_source_surface(UPSTREAM / "disasm")
    mutated = dict(source)
    path = "code/gameflow/exploration/explorationvints.asm"
    mutated[path] = mutated[path].replace("jsr     j_FieldMenu", "jsr     j_CheckArea", 1)
    with pytest.raises(ValueError, match="caller source inventory"):
        field_menu._validate_source_contract(mutated, usable_field_items_address=141794)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_field_menu_source_rejects_alternate_prompt_alias_call_order_and_equip_handoff_drift() -> (
    None
):
    source, _ = field_menu._read_source_surface(UPSTREAM / "disasm")

    alias_drift = dict(source)
    alias_path = "code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm"
    alias_drift[alias_path] = alias_drift[alias_path].replace(
        "jmp     alt_YesNoPrompt(pc)", "jmp     YesNoPrompt(pc)", 1
    )
    with pytest.raises(ValueError, match="alternate Yes/No alias"):
        field_menu._validate_source_contract(alias_drift, usable_field_items_address=141794)

    exchange_drift = dict(source)
    main_path = "code/common/menus/main/mainactions.asm"
    exchange_drift[main_path] = exchange_drift[main_path].replace(
        "jsr     j_DropItemBySlot\n                move.w  exchangedItemEntry(a6),d1\n"
        "                andi.b  #ITEMENTRY_MASK_INDEX,d1\n                jsr     j_AddItem",
        "jsr     j_AddItem\n                move.w  exchangedItemEntry(a6),d1\n"
        "                andi.b  #ITEMENTRY_MASK_INDEX,d1\n"
        "                jsr     j_DropItemBySlot",
        1,
    )
    with pytest.raises(ValueError, match="exchange call-order"):
        field_menu._validate_source_contract(exchange_drift, usable_field_items_address=141794)

    equip_drift = dict(source)
    equip_drift[main_path] = equip_drift[main_path].replace(
        "beq.w   @Goto_ExitItemEquip\n                bra.w   @ExitItemEquip",
        "beq.w   @Goto_ExitItemEquip\n                bra.w   @Goto_StartItemSubmenu",
        1,
    )
    with pytest.raises(ValueError, match="item equip handoff"):
        field_menu._validate_source_contract(equip_drift, usable_field_items_address=141794)


def test_field_menu_direct_call_parser_ignores_comments_labels_and_near_misses() -> None:
    assert field_menu._direct_calls(
        "label: jsr j_FieldMenu\n; jsr j_Hidden\njsr.w (j_CheckArea).l\n"
        "dc.l j_NotAnInstruction\njsr j_FieldMenu trailing\nbsr.s UseItemOnField\n"
    ) == ["j_CheckArea", "UseItemOnField"]


@pytest.mark.skipif(not ROM.is_file(), reason="canonical ROM is unavailable")
def test_field_menu_h1_rom_anchor_projection_rejects_one_byte_drift() -> None:
    rom = ROM.read_bytes()
    anchors = field_menu._anchor_projection(rom, rom)
    assert len(anchors) == 23
    assert anchors[6] == {
        "id": "functionAddresses.FieldMenu",
        "address": 135806,
        "width": 1902,
        "sha256": "F160CD3803063AE4E2FAD59389803B2083EE0811CA387258957F68EB11AB69ED",
        "endAddressExclusive": 137708,
    }
    assert next(anchor for anchor in anchors if anchor["id"] == "itemAction.AltYesNoPrompt") == {
        "id": "itemAction.AltYesNoPrompt",
        "address": 86668,
        "width": 2,
        "sha256": "663DD04A9305D32B3CCF48D4D3815B00393F9C5B9E1AB162EF97D06BB7F3C1B5",
    }
    drifted = bytearray(rom)
    drifted[0x2127E] ^= 1
    with pytest.raises(ValueError, match="H1/ROM anchor drift"):
        field_menu._anchor_projection(bytes(drifted), rom)


def test_field_menu_index_bindings_are_exact_and_reject_alias_roots() -> None:
    index = load_json(INDEX)
    records = {record["id"]: record for record in index["records"]}
    expected = {
        (
            "tech.interfaces.jump-s05",
            "field-menu",
            "fieldMenuSpine.entryAndCallers.jumpInterfaceAddress",
        ),
        (
            "gameflow.exploration.actions",
            "field-menu-call",
            "fieldMenuSpine.entryAndCallers.explorationCallAddress",
        ),
        (
            "gameflow.exploration.actions",
            "field-menu-return",
            "fieldMenuSpine.entryAndCallers.explorationReturnAddress",
        ),
        ("debug.battle-test", "field-menu-call", "fieldMenuSpine.entryAndCallers.debugCallAddress"),
        (
            "debug.battle-test",
            "field-menu-return",
            "fieldMenuSpine.entryAndCallers.debugReturnAddress",
        ),
        ("menus.field-main", "entry", "fieldMenuSpine.functionAddresses.FieldMenu"),
        (
            "menus.field-main",
            "populate-force-list",
            "fieldMenuSpine.functionAddresses.PopulateGenericListWithCurrentForceMembers",
        ),
        ("menus.diamond", "entry", "fieldMenuSpine.functionAddresses.ExecuteDiamondMenu"),
        ("tech.interfaces.jump-s03a", "entry", "fieldMenuSpine.jumpInterfaces.ExecuteDiamondMenu"),
        ("tech.interfaces.jump-s03a", "members-main", "fieldMenuSpine.jumpInterfaces.MembersMain"),
        ("tech.interfaces.jump-s03a", "members-item", "fieldMenuSpine.jumpInterfaces.MembersItem"),
        (
            "tech.interfaces.jump-s03a",
            "members-magic",
            "fieldMenuSpine.jumpInterfaces.MembersMagic",
        ),
        ("menus.member-list-screen", "entry", "fieldMenuSpine.functionAddresses.MembersMain"),
        (
            "menus.member-list-screen",
            "item-summary",
            "fieldMenuSpine.functionAddresses.MembersItem",
        ),
        (
            "menus.member-list-screen",
            "magic-summary",
            "fieldMenuSpine.functionAddresses.MembersMagic",
        ),
        ("menus.member-screen", "entry", "fieldMenuSpine.functionAddresses.BuildMemberScreen"),
        ("menus.yes-no-prompt", "alt-entry", "fieldMenuSpine.functionAddresses.AltYesNoPrompt"),
        (
            "menus.field-item-usability",
            "entry",
            "fieldMenuSpine.functionAddresses.IsItemUsableOnField",
        ),
        ("menus.field-item-effects", "entry", "fieldMenuSpine.functionAddresses.UseItemOnField"),
        (
            "stats.data.field-items",
            "entry",
            "fieldMenuSpine.itemAction.use.usableFieldItemsAddress",
        ),
        ("map.setup.item-event", "entry", "fieldMenuSpine.functionAddresses.RunMapSetupItemEvent"),
        ("maps.savepoint", "entry", "fieldMenuSpine.functionAddresses.GetSavepointForMap"),
        (
            "gameflow.exploration.interaction",
            "check-area",
            "fieldMenuSpine.functionAddresses.CheckArea",
        ),
        ("stats.party", "entry", "fieldMenuSpine.functionAddresses.UpdateForce"),
    }
    actual = {
        (record_id, binding["addressId"], binding["fixtureField"])
        for record_id, record in records.items()
        for evidence in record["evidence"]
        if evidence["fixtureId"] == field_menu.ID
        for binding in evidence["bindings"]
    }
    assert actual == expected
    assert len(actual) == 24
    assert all(
        "docs/research/field-menu-control.md" in record["documents"]
        for record_id, record in records.items()
        if any(record_id == expected_record for expected_record, _, _ in expected)
    )
    assert records["menus.field-main"]["documents"][-1] == "docs/research/field-item-effects.md"

    def invalid(field: str) -> None:
        broken = deepcopy(index)
        binding = next(
            binding
            for record in broken["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == field_menu.ID
            for binding in evidence["bindings"]
        )
        binding["fixtureField"] = field
        with pytest.raises(ValueError, match="schema"):
            validate_json(broken, INDEX_SCHEMA, owner="field-menu index")

    invalid("unknownRoot.fieldMenuSpine")
    invalid("sourceContext.fieldMenuSpine.entry")


@pytest.mark.skipif(
    not (UPSTREAM / "build/sf2build-h1.bin").is_file(),
    reason="pinned H1 artifact is unavailable",
)
def test_field_menu_complete_verifier_matches_fixture() -> None:
    assert field_menu.verify_field_menu_control_static(
        repo_path("local/roms/sf2-us.bin"), UPSTREAM
    ) == load_json(FIXTURE)
