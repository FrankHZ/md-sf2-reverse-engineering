from __future__ import annotations

import re
from pathlib import Path

import pytest

from sf2tool.cli import build_parser, full_verify_requested
from sf2tool.compression import decode_basic_compressed, decode_stack_compressed
from sf2tool.design_contracts import verify_design_contracts
from sf2tool.h2.battle_ai import _direct_target, _parse_source_file
from sf2tool.h2.map_content import _encode_source
from sf2tool.h2.map_descriptions import _decode_entry
from sf2tool.h2.map_entities import _record_kind
from sf2tool.h2.map_events import (
    _clean_state_event_indices,
    _decode_event_record,
    _event_matches,
)
from sf2tool.h2.map_layouts import decode_map_blocks
from sf2tool.h2.map_setup import _parse_routes, _select_route
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.research_index import listing_symbol_addresses, verify_index
from sf2tool.rom import mega_drive_checksum


def test_design_contracts_are_traceable() -> None:
    assert verify_design_contracts()["Status"] == "PASS"


def test_research_index_validates_without_private_inputs() -> None:
    result = verify_index()
    assert result["Status"] == "PASS"
    assert result["Records"] == 1535
    assert result["H2Fixtures"] == 74
    assert result["H3Fixtures"] == result["H3FixtureFiles"] == 60
    assert result["AddressBindings"] == 2109
    assert result["IndexedCodeFiles"] == 381
    assert result["IndexedDataFiles"] == 1017
    assert result["H1ListingRecords"] == 1498
    assert result["AlternateListingRecords"] == 37
    assert result["Z80MusicBankRecords"] == 37


def test_listing_symbol_addresses_indexes_once_and_rejects_conflicts() -> None:
    listing = "00000010 4E75 First:\n00000020 Second:\n00000010 First:\n"
    assert listing_symbol_addresses(listing) == {"First": 0x10, "Second": 0x20}

    with pytest.raises(ValueError, match="conflicting addresses"):
        listing_symbol_addresses("00000010 First:\n00000012 First:\n")


def test_mega_drive_checksum_handles_an_odd_trailing_byte() -> None:
    data = bytearray(0x203)
    data[0x200:] = b"\x12\x34\x56"
    assert mega_drive_checksum(bytes(data)) == "6834"


def test_verify_defaults_to_commit_profile_and_full_is_explicit() -> None:
    commit_args = build_parser().parse_args(["verify"])
    assert commit_args.command == "verify"
    assert commit_args.full is False

    full_args = build_parser().parse_args(["verify", "--full"])
    assert full_args.full is True


def test_verify_quick_remains_a_hidden_compatibility_alias() -> None:
    args = build_parser().parse_args(["verify", "--quick"])
    assert args.quick is True
    assert args.full is False
    assert full_verify_requested(args) is False


def test_legacy_skip_flags_still_select_the_full_profile() -> None:
    args = build_parser().parse_args(["verify", "--skip-runtime"])
    assert full_verify_requested(args) is True

    quick_args = build_parser().parse_args(["verify", "--quick", "--skip-runtime"])
    assert full_verify_requested(quick_args) is False


def test_growth_refresh_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "growth-refresh"])
    assert args.h3_command == "growth-refresh"
    assert args.timeout_seconds == 60


def test_enemy_gold_has_a_dedicated_narrow_extraction_command() -> None:
    args = build_parser().parse_args(["h2", "enemy-gold"])
    assert args.h2_command == "enemy-gold"
    assert args.output_path is None


def test_enemy_drops_has_a_dedicated_narrow_extraction_command() -> None:
    args = build_parser().parse_args(["h2", "enemy-drops"])
    assert args.h2_command == "enemy-drops"
    assert args.output_path is None


def test_map_script_engine_has_a_dedicated_static_contract_command() -> None:
    args = build_parser().parse_args(["h2", "map-script-engine"])
    assert args.h2_command == "map-script-engine"
    assert args.output_path is None


def test_battle_ai_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-ai"])
    assert args.h2_command == "battle-ai"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_battle_scene_engine_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-scene-engine"])
    assert args.h2_command == "battle-scene-engine"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_battle_scene_animations_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-scene-animations"])
    assert args.h2_command == "battle-scene-animations"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_battle_cutscenes_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-cutscenes"])
    assert args.h2_command == "battle-cutscenes"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_common_scripting_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "common-scripting"])
    assert args.h2_command == "common-scripting"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_common_maps_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "common-maps"])
    assert args.h2_command == "common-maps"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_common_stats_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "common-stats"])
    assert args.h2_command == "common-stats"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_common_menus_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "common-menus"])
    assert args.h2_command == "common-menus"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_tech_interrupts_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "tech-interrupts"])
    assert args.h2_command == "tech-interrupts"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_tech_graphics_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "tech-graphics"])
    assert args.h2_command == "tech-graphics"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_tech_interfaces_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "tech-interfaces"])
    assert args.h2_command == "tech-interfaces"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_tech_services_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "tech-services"])
    assert args.h2_command == "tech-services"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_gameflow_core_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "gameflow-core"])
    assert args.h2_command == "gameflow-core"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_special_screens_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "special-screens"])
    assert args.h2_command == "special-screens"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_remaining_core_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "remaining-core"])
    assert args.h2_command == "remaining-core"
    assert args.output_path is None


def test_battle_global_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-global-data"])
    assert args.h2_command == "battle-global-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_ally_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "ally-data"])
    assert args.h2_command == "ally-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_core_stats_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "core-stats-data"])
    assert args.h2_command == "core-stats-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_item_auxiliary_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "item-auxiliary"])
    assert args.h2_command == "item-auxiliary"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_enemy_map_sprites_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "enemy-map-sprites"])
    assert args.h2_command == "enemy-map-sprites"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_map_sprite_assignments_have_a_static_audit_command() -> None:
    args = build_parser().parse_args(["h2", "map-sprite-assignments"])
    assert args.h2_command == "map-sprite-assignments"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_entity_action_scripts_have_a_static_corpus_command() -> None:
    args = build_parser().parse_args(["h2", "entity-action-scripts"])
    assert args.h2_command == "entity-action-scripts"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_sprite_dialogue_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "sprite-dialogue"])
    assert args.h2_command == "sprite-dialogue"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_battle_cutscene_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-cutscene-data"])
    assert args.h2_command == "battle-cutscene-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_battle_spriteset_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-spriteset-data"])
    assert args.h2_command == "battle-spriteset-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_battle_routing_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-routing-data"])
    assert args.h2_command == "battle-routing-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_battle_terrain_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "battle-terrain"])
    assert args.h2_command == "battle-terrain"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_battle_backgrounds_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "battle-backgrounds"])
    assert args.h2_command == "battle-backgrounds"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_battle_sprites_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "battle-sprites"])
    assert args.h2_command == "battle-sprites"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_battle_sprite_animations_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "battle-sprite-animations"])
    assert args.h2_command == "battle-sprite-animations"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_battle_weapon_ground_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "battle-weapon-ground"])
    assert args.h2_command == "battle-weapon-ground"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_stack_decoder_handles_literal_overlap_copy_and_terminator() -> None:
    def packed(bits: str) -> bytes:
        bits += "0" * (-len(bits) % 8)
        return int(bits, 2).to_bytes(len(bits) // 8, "big")

    # Custom command nibble 6 followed by three zero nibbles gives command word 0x6000:
    # literal zero, offset-one copy of two words, then the zero-offset terminator.
    stream = packed("11110110" + "000" + "00" * 4 + f"{1:011b}" + "1" + "0" * 11)
    result = decode_stack_compressed(stream, expected_output_bytes=6)
    assert result.output == bytes(6)
    assert result.literal_word_count == 1
    assert result.copy_command_count == 1
    assert result.copied_word_count == 2
    assert result.maximum_copy_offset_words == 1
    assert result.maximum_copy_length_words == 2


def test_basic_decoder_handles_literal_overlap_copy_and_terminator() -> None:
    # Bitmap 0,1,1: literal 0x1234, offset-one/length-two copy, terminator.
    stream = bytes.fromhex("6000 1234 003F 0000")
    result = decode_basic_compressed(stream, expected_output_bytes=6)
    assert result.output == bytes.fromhex("1234 1234 1234")
    assert result.command_word_count == 1
    assert result.literal_word_count == 1
    assert result.copy_command_count == 1
    assert result.copied_word_count == 2
    assert result.repeat_last_word_command_count == 1
    assert result.maximum_copy_offset_words == 1
    assert result.maximum_copy_length_words == 2


def test_portraits_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "portraits"])
    assert args.h2_command == "portraits"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_map_sprites_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-sprites"])
    assert args.h2_command == "map-sprites"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_special_sprites_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "special-sprites"])
    assert args.h2_command == "special-sprites"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_special_screen_graphics_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "special-screen-graphics"])
    assert args.h2_command == "special-screen-graphics"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_special_screen_presentation_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "special-screen-presentation"])
    assert args.h2_command == "special-screen-presentation"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_ui_graphics_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "ui-graphics"])
    assert args.h2_command == "ui-graphics"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_ui_layouts_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "ui-layouts"])
    assert args.h2_command == "ui-layouts"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_variable_width_font_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "variable-width-font"])
    assert args.h2_command == "variable-width-font"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_text_huffman_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "text-huffman"])
    assert args.h2_command == "text-huffman"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_text_banks_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "text-banks"])
    assert args.h2_command == "text-banks"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_unused_technical_assets_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "unused-tech-assets"])
    assert args.h2_command == "unused-tech-assets"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_witch_menu_graphics_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "witch-menu-graphics"])
    assert args.h2_command == "witch-menu-graphics"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_icon_graphics_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "icon-graphics"])
    assert args.h2_command == "icon-graphics"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_battle_effect_graphics_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "battle-effect-graphics"])
    assert args.h2_command == "battle-effect-graphics"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_map_tilesets_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-tilesets"])
    assert args.h2_command == "map-tilesets"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_compression_consumers_have_a_source_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "compression-consumers"])
    assert args.h2_command == "compression-consumers"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_map_palettes_have_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-palettes"])
    assert args.h2_command == "map-palettes"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_map_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "map-data"])
    assert args.h2_command == "map-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_map_setup_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-setup"])
    assert args.h2_command == "map-setup"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_map_setup_selection_uses_the_last_set_flag_in_source_order() -> None:
    routes = _parse_routes(
        "MapSetups: msMap 3, ms_map3\n"
        " msFlag 10, ms_map3_flag10\n"
        " msFlag 20, ms_map3_flag20\n"
        " msMapEnd\n"
        " msEnd\n"
    )
    assert _select_route(routes, 3, set()) == "ms_map3"
    assert _select_route(routes, 3, {10, 20}) == "ms_map3_flag20"
    assert _select_route(routes, 4, {10, 20}) == "ms_Void"


def test_map_event_dispatch_has_one_batched_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "map-event-dispatch"])
    assert args.h3_command == "map-event-dispatch"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.timeout_seconds == 120


def test_map_animation_vdp_has_one_batched_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "map-animation-vdp"])
    assert args.h3_command == "map-animation-vdp"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.timeout_seconds == 120


def test_sound_timing_has_one_batched_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "sound-timing"])
    assert args.h3_command == "sound-timing"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.timeout_seconds == 120


def test_entity_movement_has_one_batched_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "entity-movement"])
    assert args.h3_command == "entity-movement"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.timeout_seconds == 120


def test_map_entities_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-entities"])
    assert args.h2_command == "map-entities"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_map_content_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-content"])
    assert args.h2_command == "map-content"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_map_content_encoder_keeps_item_terminator_and_trailing_rts(tmp_path: Path) -> None:
    source = tmp_path / "8-other-items.asm"
    source.write_text(
        "mapItem 1, 2, 3, HEALING_SEED\nendWord\nrts\n",
        encoding="utf-8",
    )
    encoded, records, trailing_rts = _encode_source(
        source,
        "otherItems",
        {"ITEM_HEALING_SEED": 1},
    )
    assert encoded == bytes.fromhex("01020301FFFF4E75")
    assert records == 1
    assert trailing_rts is True


def test_map_layouts_has_a_static_decode_command() -> None:
    args = build_parser().parse_args(["h2", "map-layouts"])
    assert args.h2_command == "map-layouts"
    assert args.rom_path.name == "sf2-us.bin"


def test_zero_command_block_stream_yields_the_three_builtin_blocks() -> None:
    words, consumed_bits, commands = decode_map_blocks(b"\x00\x00")
    assert len(words) == 27
    assert consumed_bits == 14
    assert commands == {}


def test_map_entity_payload_prefix_classifies_record_encoding() -> None:
    assert _record_kind(bytes.fromhex("0102030400001234")) == "fixed"
    assert _record_kind(bytes.fromhex("01020304FF050607")) == "walking"
    assert _record_kind(bytes.fromhex("01020304FE001234")) == "sequenced"


def test_map_events_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-events"])
    assert args.h2_command == "map-events"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_map_entity_event_indices_split_allies_from_stream_order_enemies() -> None:
    records = [
        {"mapSprite": 2},
        {"mapSprite": 122},
        {"mapSprite": 5},
        {"mapSprite": 132},
    ]
    assert _clean_state_event_indices(records) == [2, 128, 5, 129]


def test_map_event_matcher_handles_wildcards_defaults_and_item_mask() -> None:
    assert _event_matches(
        "zoneEvents", {"kind": "specific", "x": 0xFF, "y": 12}, {"x": 7, "y": 12}
    )
    assert _event_matches(
        "itemEvents",
        {"kind": "specific", "x": 1, "y": 2, "facing": 0xFF, "item": 112},
        {"x": 1, "y": 2, "facing": 3, "item": 240},
    )
    assert _event_matches("entityEvents", {"kind": "default"}, {"entity": 254})


def test_map_event_relative_offsets_resolve_from_table_base() -> None:
    record = _decode_event_record("zoneEvents", 0x1000, 0x1004, bytes.fromhex("FD000020"))
    assert record == {
        "address": 0x1004,
        "kind": "default",
        "relativeOffset": 0x20,
        "resolvedTargetAddress": 0x1020,
        "x": 0xFD,
        "y": 0,
    }


def test_map_descriptions_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-descriptions"])
    assert args.h2_command == "map-descriptions"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_map_description_text_indices_use_global_and_wrapper_bases() -> None:
    record = _decode_entry(0x1000, 0x1006, bytes.fromhex("010200000304"), 0x200)
    assert record["kind"] == "text"
    assert record["investigationTextIndex"] == 426
    assert record["descriptionTextIndex"] == 0x204


def test_map_init_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "map-init"])
    assert args.h2_command == "map-init"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_map_scripts_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "map-scripts"])
    assert args.h2_command == "map-scripts"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_auxiliary_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "auxiliary-data"])
    assert args.h2_command == "auxiliary-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_sound_data_has_a_static_rom_parity_command() -> None:
    args = build_parser().parse_args(["h2", "sound-data"])
    assert args.h2_command == "sound-data"
    assert args.rom_path.name == "sf2-us.bin"
    assert args.output_path is None


def test_battle_ai_inventory_classifies_calls(tmp_path: Path) -> None:
    source = tmp_path / "sample.asm"
    source.write_text(
        "Entry: bsr.w DirectCall\n"
        "@Loop: jsr (OtherCall).w\n"
        "        jsr (a0)\n"
        "loc_1234:\n"
        "        rts\n",
        encoding="utf-8",
    )
    parsed = _parse_source_file(source, "code/gameflow/battle/ai/sample.asm")
    assert parsed["globalLabels"] == ["Entry", "loc_1234"]
    assert parsed["localLabelCount"] == 1
    assert parsed["statementCount"] == 4
    assert parsed["directCalls"] == [
        {"target": "DirectCall", "siteCount": 1},
        {"target": "OtherCall", "siteCount": 1},
    ]
    assert parsed["indirectCallSiteCount"] == 1
    assert _direct_target("4(a0)") is None


def test_source_inventory_accepts_legacy_single_byte_comments(tmp_path: Path) -> None:
    source = tmp_path / "legacy.asm"
    source.write_bytes(b"Entry: and 80h ; '\x80\x90'\r\n        rts\r\n")

    parsed = _parse_source_file(source, "code/common/tech/sound/legacy.asm")

    assert parsed["globalLabels"] == ["Entry"]
    assert parsed["statementCount"] == 2


def test_kill_exp_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "kill-exp"])
    assert args.h3_command == "kill-exp"
    assert args.timeout_seconds == 75


def test_award_exp_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "award-exp"])
    assert args.h3_command == "award-exp"
    assert args.timeout_seconds == 90


def test_exp_command_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "exp-command"])
    assert args.h3_command == "exp-command"
    assert args.timeout_seconds == 75


def test_gold_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "gold"])
    assert args.h3_command == "gold"
    assert args.timeout_seconds == 60


def test_enemy_drops_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "enemy-drops"])
    assert args.h3_command == "enemy-drops"
    assert args.timeout_seconds == 75


def test_muddle_action_guard_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "muddle-action-guard"])
    assert args.h3_command == "muddle-action-guard"
    assert args.timeout_seconds == 120


def test_legacy_powershell_surface_does_not_expand() -> None:
    root = Path(__file__).resolve().parents[2]
    scripts = sorted((root / "scripts").rglob("*.ps1"))
    lines = sum(len(path.read_text(encoding="utf-8").splitlines()) for path in scripts)
    assert len(scripts) <= 36
    assert lines <= 4813


def test_tracked_lua_does_not_use_reserved_words_as_dot_fields() -> None:
    root = Path(__file__).resolve().parents[2]
    keywords = (
        "and|break|do|else|elseif|end|false|for|function|goto|if|in|local|nil|not|or|"
        "repeat|return|then|true|until|while"
    )
    pattern = re.compile(rf"\.\s*(?:{keywords})\b")
    failures = []
    for path in sorted((root / "tools" / "bizhawk").glob("*.lua")):
        for line_number, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
            if pattern.search(line):
                failures.append(f"{path.name}:{line_number}: {line.strip()}")
    assert not failures, "Lua reserved word used after '.':\n" + "\n".join(failures)


def test_bizhawk_lua_preflight_compiles_observers_and_rejects_syntax_errors(
    tmp_path: Path,
) -> None:
    try:
        _, executable = bizhawk_contract()
    except FileNotFoundError:
        pytest.skip("local BizHawk toolchain is not installed")

    root = Path(__file__).resolve().parents[2]
    for path in sorted((root / "tools" / "bizhawk").glob("*.lua")):
        validate_lua_syntax(path, executable)

    invalid = tmp_path / "reserved-field.lua"
    invalid.write_text("local config = {}\nreturn config.function\n", encoding="utf-8")
    with pytest.raises(ValueError, match=r"expected near 'function'"):
        validate_lua_syntax(invalid, executable)
