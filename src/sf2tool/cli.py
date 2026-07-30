from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path
from typing import Any

from sf2tool.design_contracts import verify_design_contracts
from sf2tool.h2.ally_data import verify_ally_data_inventory
from sf2tool.h2.auxiliary_data import verify_auxiliary_data_inventory
from sf2tool.h2.battle_actions import verify_battle_actions_inventory
from sf2tool.h2.battle_ai import verify_battle_ai_inventory
from sf2tool.h2.battle_backgrounds import verify_battle_background_contract
from sf2tool.h2.battle_control import verify_battle_control_inventory
from sf2tool.h2.battle_cutscene_data import verify_battle_cutscene_data_inventory
from sf2tool.h2.battle_cutscenes import verify_battle_cutscene_inventory
from sf2tool.h2.battle_effect_graphics import verify_battle_effect_graphics_contract
from sf2tool.h2.battle_functions import verify_battle_functions_inventory
from sf2tool.h2.battle_global_data import verify_battle_global_data_inventory
from sf2tool.h2.battle_loop import verify_battle_loop_inventory
from sf2tool.h2.battle_routing_data import verify_battle_routing_data_inventory
from sf2tool.h2.battle_scene_animations import verify_battle_scene_animation_inventory
from sf2tool.h2.battle_scene_engine import verify_battle_scene_engine_inventory
from sf2tool.h2.battle_sprite_animations import verify_battle_sprite_animation_contract
from sf2tool.h2.battle_sprites import verify_battle_sprite_contract
from sf2tool.h2.battle_spriteset_data import verify_battle_spriteset_data_inventory
from sf2tool.h2.battle_terrain import verify_battle_terrain_contract
from sf2tool.h2.battle_weapon_ground import verify_battle_weapon_ground_contract
from sf2tool.h2.battlefield import verify_battlefield_inventory
from sf2tool.h2.compression_consumers import verify_compression_consumer_inventory
from sf2tool.h2.core_stats_data import verify_core_stats_data_inventory
from sf2tool.h2.enemy_drops import verify_enemy_item_drops
from sf2tool.h2.enemy_gold import verify_enemy_gold
from sf2tool.h2.enemy_map_sprites import verify_enemy_map_sprites_contract
from sf2tool.h2.entity_action_scripts import verify_entity_action_script_contract
from sf2tool.h2.gameflow import verify_gameflow_inventory
from sf2tool.h2.graphics import verify_graphics_inventory
from sf2tool.h2.icon_graphics import verify_icon_graphics_contract
from sf2tool.h2.interfaces import verify_interface_inventory
from sf2tool.h2.interrupts import verify_interrupt_inventory
from sf2tool.h2.item_auxiliary import verify_item_auxiliary_contract
from sf2tool.h2.map_content import verify_map_content_contract
from sf2tool.h2.map_data import verify_map_data_inventory
from sf2tool.h2.map_descriptions import verify_map_descriptions_contract
from sf2tool.h2.map_entities import verify_map_entities_contract
from sf2tool.h2.map_events import verify_map_events_contract
from sf2tool.h2.map_import import verify_canonical_map_import
from sf2tool.h2.map_init import verify_map_init_contract
from sf2tool.h2.map_layouts import verify_map_layout_contract
from sf2tool.h2.map_palettes import verify_map_palette_contract
from sf2tool.h2.map_script_engine import verify_map_script_engine_contract
from sf2tool.h2.map_scripts import verify_map_scripts_inventory
from sf2tool.h2.map_setup import verify_map_setup_contract
from sf2tool.h2.map_sprite_assignments import verify_map_sprite_assignment_contract
from sf2tool.h2.map_sprites import verify_map_sprite_contract
from sf2tool.h2.map_tilesets import verify_map_tileset_contract
from sf2tool.h2.maps import verify_map_inventory
from sf2tool.h2.menus import verify_menu_inventory
from sf2tool.h2.portraits import verify_portrait_graphics_contract
from sf2tool.h2.remaining_core import verify_remaining_core_inventory
from sf2tool.h2.screens import verify_special_screen_inventory
from sf2tool.h2.scripting import verify_scripting_inventory
from sf2tool.h2.services import verify_service_inventory
from sf2tool.h2.sound_data import verify_sound_data_inventory
from sf2tool.h2.special_screen_graphics import verify_special_screen_graphics_contract
from sf2tool.h2.special_screen_presentation import verify_special_screen_presentation_contract
from sf2tool.h2.special_sprites import verify_special_sprite_contract
from sf2tool.h2.sprite_dialogue import verify_sprite_dialogue_contract
from sf2tool.h2.stats import verify_stats_inventory
from sf2tool.h2.text_banks import verify_text_banks_contract
from sf2tool.h2.text_huffman import verify_text_huffman_contract
from sf2tool.h2.ui_graphics import verify_ui_graphics_contract
from sf2tool.h2.ui_layouts import verify_ui_layout_contract
from sf2tool.h2.unused_technical_assets import verify_unused_technical_assets_contract
from sf2tool.h2.variable_width_font import verify_variable_width_font_contract
from sf2tool.h2.witch_menu_graphics import verify_witch_menu_graphics_contract
from sf2tool.h3.after_turn import verify_after_turn_status_lifecycle
from sf2tool.h3.award_exp import verify_award_exp_randomization
from sf2tool.h3.battle_ai_action import verify_battle_ai_action_choice
from sf2tool.h3.battle_exp import verify_battle_exp_level_up
from sf2tool.h3.battlefield_matrix import verify_battlefield_movement_matrix
from sf2tool.h3.enemy_curse import verify_enemy_curse_suppression
from sf2tool.h3.enemy_drops import verify_enemy_item_drop_behavior
from sf2tool.h3.entity_movement import verify_entity_movement_matrix
from sf2tool.h3.exp_command import verify_exp_command_boundaries
from sf2tool.h3.gold import verify_gold_boundaries
from sf2tool.h3.growth import (
    verify_growth,
    verify_initialization_prowess,
    verify_level_up_refresh,
)
from sf2tool.h3.kill_exp import verify_kill_exp_level_differences
from sf2tool.h3.map_animation_vdp import verify_map_animation_vdp
from sf2tool.h3.map_event_dispatch import verify_map_event_dispatch
from sf2tool.h3.map_init_dispatch import verify_map_init_dispatch
from sf2tool.h3.map_interaction_trigger import verify_map_interaction_trigger
from sf2tool.h3.map_lifecycle import verify_map_lifecycle
from sf2tool.h3.map_setup_selection import verify_map_setup_selection
from sf2tool.h3.muddle_action_guard import verify_muddle_action_guard
from sf2tool.h3.muddle_confusion import verify_muddle_confusion
from sf2tool.h3.rng import verify_rng
from sf2tool.h3.sound_timing import verify_sound_timing
from sf2tool.h3.spell_attack import verify_spell_attack
from sf2tool.h3.spell_boost import verify_spell_boost
from sf2tool.h3.spell_damage import verify_spell_damage, verify_spell_summon
from sf2tool.h3.spell_desoul import verify_spell_desoul
from sf2tool.h3.spell_detox import verify_spell_detox
from sf2tool.h3.spell_dispel import verify_spell_dispel
from sf2tool.h3.spell_exp import verify_spell_damage_exp
from sf2tool.h3.spell_healing import (
    verify_spell_aura,
    verify_spell_healing,
    verify_spell_healing_exp,
)
from sf2tool.h3.spell_mp import verify_spell_mp_absorb
from sf2tool.h3.spell_muddle import verify_spell_muddle, verify_spell_muddle1
from sf2tool.h3.spell_silence import verify_spell_silence_gate
from sf2tool.h3.spell_slow import verify_spell_slow
from sf2tool.h3.spell_status import verify_spell_status
from sf2tool.h3.stat_clamps import verify_stat_clamp_boundaries
from sf2tool.h3.witch_new_game_lifecycle import verify_witch_new_game_lifecycle
from sf2tool.h3.witch_save_actions import verify_witch_save_actions
from sf2tool.harness import verify
from sf2tool.legacy import run_powershell
from sf2tool.output import print_json, print_record
from sf2tool.paths import repo_path
from sf2tool.research_index import index_rows, index_summary, query_index, verify_index
from sf2tool.rom import verify_rom

DEFAULT_ROM = repo_path("local/roms/sf2-us.bin")
DEFAULT_UPSTREAM = repo_path("local/upstream/SF2DISASM")


def _path(value: str) -> Path:
    return Path(value)


def _add_local_paths(parser: argparse.ArgumentParser, *, rom: bool = True) -> None:
    if rom:
        parser.add_argument("--rom-path", type=_path, default=DEFAULT_ROM)
    parser.add_argument("--upstream-path", type=_path, default=DEFAULT_UPSTREAM)


def _print_rows(rows: list[dict[str, Any]]) -> None:
    if not rows:
        return
    columns = list(rows[0])
    widths = {
        column: max(len(column), *(len(str(row[column])) for row in rows)) for column in columns
    }
    print("  ".join(f"{column:<{widths[column]}}" for column in columns))
    print("  ".join("-" * widths[column] for column in columns))
    for row in rows:
        print("  ".join(f"{str(row[column]):<{widths[column]}}" for column in columns))


def full_verify_requested(args: argparse.Namespace) -> bool:
    if args.quick:
        return False
    return args.full or args.skip_rebuild or args.skip_extraction or args.skip_runtime


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="sf2", description="Shining Force II research harness")
    commands = parser.add_subparsers(dest="command", required=True)

    verify_parser = commands.add_parser("verify", help="run the repository verification rails")
    _add_local_paths(verify_parser)
    verify_parser.add_argument(
        "--skip-rebuild", action="store_true", help="full profile: omit the H1 rebuild"
    )
    verify_parser.add_argument(
        "--skip-extraction", action="store_true", help="full profile: omit H2 extraction rails"
    )
    verify_parser.add_argument(
        "--skip-runtime", action="store_true", help="full profile: omit H3 runtime rails"
    )
    verify_profile = verify_parser.add_mutually_exclusive_group()
    verify_profile.add_argument(
        "--full",
        action="store_true",
        help="enable the milestone profile with H1, H2, and H3 rails",
    )
    verify_profile.add_argument("--quick", action="store_true", help=argparse.SUPPRESS)

    init_parser = commands.add_parser("init", help="initialize ignored local research inputs")
    init_parser.add_argument("--rom-path", type=_path, required=True)

    rom_parser = commands.add_parser("rom", help="inspect or verify the ROM baseline")
    rom_commands = rom_parser.add_subparsers(dest="rom_command", required=True)
    rom_verify = rom_commands.add_parser("verify")
    rom_verify.add_argument("--rom-path", type=_path, default=DEFAULT_ROM)

    index_parser = commands.add_parser("research-index", aliases=["index"])
    index_commands = index_parser.add_subparsers(dest="index_command", required=True)
    index_test = index_commands.add_parser("test")
    index_test.add_argument("--upstream-path", type=_path, default=DEFAULT_UPSTREAM)
    index_list = index_commands.add_parser("list")
    index_list.add_argument("--query")
    index_list.add_argument("--subsystem")
    index_list.add_argument("--status", choices=("confirmed", "inferred", "unknown"))
    index_list.add_argument("--fixture")
    index_list.add_argument("--summary", action="store_true")
    index_list.add_argument("--json", action="store_true")

    design_parser = commands.add_parser("design-contracts")
    design_parser.add_subparsers(dest="design_command", required=True).add_parser("test")

    h2_parser = commands.add_parser("h2", help="run a narrow deterministic extraction rail")
    h2_commands = h2_parser.add_subparsers(dest="h2_command", required=True)
    h2_enemy_gold = h2_commands.add_parser(
        "enemy-gold", help="extract and byte-compare the enemy gold table"
    )
    _add_local_paths(h2_enemy_gold)
    h2_enemy_gold.add_argument("--output-path", type=_path)
    h2_enemy_drops = h2_commands.add_parser(
        "enemy-drops", help="extract and byte-compare the enemy item drop table"
    )
    _add_local_paths(h2_enemy_drops)
    h2_enemy_drops.add_argument("--output-path", type=_path)
    h2_battle_ai = h2_commands.add_parser(
        "battle-ai", help="inventory the complete battle AI source subtree"
    )
    _add_local_paths(h2_battle_ai, rom=False)
    h2_battle_ai.add_argument("--output-path", type=_path)
    h2_battlefield = h2_commands.add_parser(
        "battlefield", help="inventory the complete battlefield/pathfinding source subtree"
    )
    _add_local_paths(h2_battlefield, rom=False)
    h2_battlefield.add_argument("--output-path", type=_path)
    h2_battle_loop = h2_commands.add_parser(
        "battle-loop", help="inventory the complete battle-loop and lifecycle source subtree"
    )
    _add_local_paths(h2_battle_loop, rom=False)
    h2_battle_loop.add_argument("--output-path", type=_path)
    h2_battle_control = h2_commands.add_parser(
        "battle-control", help="inventory top-level battle loop and shared control sources"
    )
    _add_local_paths(h2_battle_control, rom=False)
    h2_battle_control.add_argument("--output-path", type=_path)
    h2_battle_actions = h2_commands.add_parser(
        "battle-actions", help="inventory battle action script construction and effect sources"
    )
    _add_local_paths(h2_battle_actions, rom=False)
    h2_battle_actions.add_argument("--output-path", type=_path)
    h2_battle_functions = h2_commands.add_parser(
        "battle-functions", help="inventory shared battle turn, cursor, load, and exit functions"
    )
    _add_local_paths(h2_battle_functions, rom=False)
    h2_battle_functions.add_argument("--output-path", type=_path)
    h2_battle_scene_engine = h2_commands.add_parser(
        "battle-scene-engine", help="inventory root battle scene engine and dispatch sources"
    )
    _add_local_paths(h2_battle_scene_engine, rom=False)
    h2_battle_scene_engine.add_argument("--output-path", type=_path)
    h2_battle_scene_animations = h2_commands.add_parser(
        "battle-scene-animations", help="inventory and pair all battle scene animations"
    )
    _add_local_paths(h2_battle_scene_animations, rom=False)
    h2_battle_scene_animations.add_argument("--output-path", type=_path)
    h2_battle_cutscenes = h2_commands.add_parser(
        "battle-cutscenes", help="inventory battle cutscene routing and admission sources"
    )
    _add_local_paths(h2_battle_cutscenes, rom=False)
    h2_battle_cutscenes.add_argument("--output-path", type=_path)
    h2_common_scripting = h2_commands.add_parser(
        "common-scripting", help="inventory map, entity, text, and credits scripting engines"
    )
    _add_local_paths(h2_common_scripting, rom=False)
    h2_common_scripting.add_argument("--output-path", type=_path)
    h2_common_maps = h2_commands.add_parser(
        "common-maps", help="inventory map routing, loading, camera, and animation sources"
    )
    _add_local_paths(h2_common_maps, rom=False)
    h2_common_maps.add_argument("--output-path", type=_path)
    h2_common_stats = h2_commands.add_parser(
        "common-stats", help="inventory flags, party, inventory, item, spell, and stat services"
    )
    _add_local_paths(h2_common_stats, rom=False)
    h2_common_stats.add_argument("--output-path", type=_path)
    h2_common_menus = h2_commands.add_parser(
        "common-menus", help="inventory menu engines, prompts, services, and presentation sources"
    )
    _add_local_paths(h2_common_menus, rom=False)
    h2_common_menus.add_argument("--output-path", type=_path)
    h2_tech_interrupts = h2_commands.add_parser(
        "tech-interrupts", help="inventory VInt, DMA, fading, traps, and technical handlers"
    )
    _add_local_paths(h2_tech_interrupts, rom=False)
    h2_tech_interrupts.add_argument("--output-path", type=_path)
    h2_tech_graphics = h2_commands.add_parser(
        "tech-graphics", help="inventory decompression, display, palette, and sprite services"
    )
    _add_local_paths(h2_tech_graphics, rom=False)
    h2_tech_graphics.add_argument("--output-path", type=_path)
    h2_tech_interfaces = h2_commands.add_parser(
        "tech-interfaces", help="inventory cross-section jump stubs and longword pointers"
    )
    _add_local_paths(h2_tech_interfaces, rom=False)
    h2_tech_interfaces.add_argument("--output-path", type=_path)
    h2_tech_services = h2_commands.add_parser(
        "tech-services", help="inventory remaining resources, sound, SRAM, input, copy, and RNG"
    )
    _add_local_paths(h2_tech_services, rom=False)
    h2_tech_services.add_argument("--output-path", type=_path)
    h2_gameflow_core = h2_commands.add_parser(
        "gameflow-core", help="inventory startup, main-loop, and exploration sources"
    )
    _add_local_paths(h2_gameflow_core, rom=False)
    h2_gameflow_core.add_argument("--output-path", type=_path)
    h2_special_screens = h2_commands.add_parser(
        "special-screens", help="inventory logo, title, witch, suspend, and ending screens"
    )
    _add_local_paths(h2_special_screens, rom=False)
    h2_special_screens.add_argument("--output-path", type=_path)
    h2_remaining_core = h2_commands.add_parser(
        "remaining-core", help="inventory ROM header, window engine, and special debug flows"
    )
    _add_local_paths(h2_remaining_core, rom=False)
    h2_remaining_core.add_argument("--output-path", type=_path)
    h2_battle_global_data = h2_commands.add_parser(
        "battle-global-data", help="inventory global battle data tables and layout ownership"
    )
    _add_local_paths(h2_battle_global_data, rom=False)
    h2_battle_global_data.add_argument("--output-path", type=_path)
    h2_ally_data = h2_commands.add_parser(
        "ally-data", help="inventory ally, class, growth, and spell-learning data"
    )
    _add_local_paths(h2_ally_data, rom=False)
    h2_ally_data.add_argument("--output-path", type=_path)
    h2_core_stats_data = h2_commands.add_parser(
        "core-stats-data", help="inventory item, spell, and enemy data tables"
    )
    _add_local_paths(h2_core_stats_data, rom=False)
    h2_core_stats_data.add_argument("--output-path", type=_path)
    h2_item_auxiliary = h2_commands.add_parser(
        "item-auxiliary",
        help="verify shops, mithril, chest, field-item, break-message, and weapon graphics tables",
    )
    _add_local_paths(h2_item_auxiliary)
    h2_item_auxiliary.add_argument("--output-path", type=_path)
    h2_enemy_map_sprites = h2_commands.add_parser(
        "enemy-map-sprites",
        help="verify all enemy map-sprite rows and the normal-vs-tail index boundary",
    )
    _add_local_paths(h2_enemy_map_sprites)
    h2_enemy_map_sprites.add_argument("--output-path", type=_path)
    h2_entity_action_scripts = h2_commands.add_parser(
        "entity-action-scripts",
        help="parse the complete entity-action corpus, control flow, and dispatcher handlers",
    )
    _add_local_paths(h2_entity_action_scripts)
    h2_entity_action_scripts.add_argument("--output-path", type=_path)
    h2_map_script_engine = h2_commands.add_parser(
        "map-script-engine", help="inventory map-script macros, handlers, and source usage"
    )
    _add_local_paths(h2_map_script_engine)
    h2_map_script_engine.add_argument("--output-path", type=_path)
    h2_sprite_dialogue = h2_commands.add_parser(
        "sprite-dialogue",
        help="decode map-sprite portrait and speech-SFX properties against the ROM",
    )
    _add_local_paths(h2_sprite_dialogue)
    h2_sprite_dialogue.add_argument("--output-path", type=_path)
    h2_battle_cutscene_data = h2_commands.add_parser(
        "battle-cutscene-data", help="inventory built and orphaned battle cutscene data"
    )
    _add_local_paths(h2_battle_cutscene_data, rom=False)
    h2_battle_cutscene_data.add_argument("--output-path", type=_path)
    h2_battle_spriteset_data = h2_commands.add_parser(
        "battle-spriteset-data", help="inventory battle roster, placement, and AI-region tables"
    )
    _add_local_paths(h2_battle_spriteset_data, rom=False)
    h2_battle_spriteset_data.add_argument("--output-path", type=_path)
    h2_battle_routing_data = h2_commands.add_parser(
        "battle-routing-data", help="inventory battle cutscene routing and terrain containers"
    )
    _add_local_paths(h2_battle_routing_data, rom=False)
    h2_battle_routing_data.add_argument("--output-path", type=_path)
    h2_battle_terrain = h2_commands.add_parser(
        "battle-terrain", help="decode every Stack-compressed 48x48 battle terrain grid"
    )
    _add_local_paths(h2_battle_terrain)
    h2_battle_terrain.add_argument("--output-path", type=_path)
    h2_battle_backgrounds = h2_commands.add_parser(
        "battle-backgrounds",
        help="decode every battle background palette and pair of Stack-compressed tilesets",
    )
    _add_local_paths(h2_battle_backgrounds)
    h2_battle_backgrounds.add_argument("--output-path", type=_path)
    h2_battle_sprites = h2_commands.add_parser(
        "battle-sprites",
        help="decode every ally and enemy battle-sprite palette and frame container",
    )
    _add_local_paths(h2_battle_sprites)
    h2_battle_sprites.add_argument("--output-path", type=_path)
    h2_battle_sprite_animations = h2_commands.add_parser(
        "battle-sprite-animations",
        help="verify all ally/enemy battle-sprite animation tables, payloads, and selector rules",
    )
    _add_local_paths(h2_battle_sprite_animations)
    h2_battle_sprite_animations.add_argument("--output-path", type=_path)
    h2_battle_weapon_ground = h2_commands.add_parser(
        "battle-weapon-ground",
        help="decode battle weapon sprites, weapon palettes, and ground graphics containers",
    )
    _add_local_paths(h2_battle_weapon_ground)
    h2_battle_weapon_ground.add_argument("--output-path", type=_path)
    h2_portraits = h2_commands.add_parser(
        "portraits", help="decode all portrait metadata, palettes, and Stack-compressed tiles"
    )
    _add_local_paths(h2_portraits)
    h2_portraits.add_argument("--output-path", type=_path)
    h2_map_data = h2_commands.add_parser(
        "map-data", help="inventory the complete map ASM include graph and internal symbols"
    )
    _add_local_paths(h2_map_data, rom=False)
    h2_map_data.add_argument("--output-path", type=_path)
    h2_map_content = h2_commands.add_parser(
        "map-content",
        help="re-encode all map content tables and byte-compare private blocks/layouts with ROM",
    )
    _add_local_paths(h2_map_content)
    h2_map_content.add_argument("--output-path", type=_path)
    h2_map_layouts = h2_commands.add_parser(
        "map-layouts", help="decode every compressed map blockset and 64x64 layout"
    )
    _add_local_paths(h2_map_layouts)
    h2_map_layouts.add_argument("--output-path", type=_path)
    h2_map_sprites = h2_commands.add_parser(
        "map-sprites", help="decode the complete Basic-compressed map-sprite pointer corpus"
    )
    _add_local_paths(h2_map_sprites)
    h2_map_sprites.add_argument("--output-path", type=_path)
    h2_map_sprite_assignments = h2_commands.add_parser(
        "map-sprite-assignments",
        help="audit initial, scripted, ally/enemy-derived, and direct map-sprite writes",
    )
    _add_local_paths(h2_map_sprite_assignments)
    h2_map_sprite_assignments.add_argument("--output-path", type=_path)
    h2_special_sprites = h2_commands.add_parser(
        "special-sprites",
        help="decode the complete Stack-compressed special-sprite corpus and routing boundary",
    )
    _add_local_paths(h2_special_sprites)
    h2_special_sprites.add_argument("--output-path", type=_path)
    h2_special_screen_graphics = h2_commands.add_parser(
        "special-screen-graphics",
        help="decode every Stack-compressed tile resource consumed by special-screen code",
    )
    _add_local_paths(h2_special_screen_graphics)
    h2_special_screen_graphics.add_argument("--output-path", type=_path)
    h2_special_screen_presentation = h2_commands.add_parser(
        "special-screen-presentation",
        help="verify all uncompressed special-screen palettes and layouts against ROM",
    )
    _add_local_paths(h2_special_screen_presentation)
    h2_special_screen_presentation.add_argument("--output-path", type=_path)
    h2_ui_graphics = h2_commands.add_parser(
        "ui-graphics",
        help="decode the complete base, diamond-menu, and yes/no Stack-compressed corpus",
    )
    _add_local_paths(h2_ui_graphics)
    h2_ui_graphics.add_argument("--output-path", type=_path)
    h2_ui_layouts = h2_commands.add_parser(
        "ui-layouts",
        help="verify all assembled UI/window layouts, pointer routes, borders, and direct assets",
    )
    _add_local_paths(h2_ui_layouts)
    h2_ui_layouts.add_argument("--output-path", type=_path)
    h2_variable_width_font = h2_commands.add_parser(
        "variable-width-font",
        help="verify the variable-width glyph corpus, ASCII map, pointer, and consumers",
    )
    _add_local_paths(h2_variable_width_font)
    h2_variable_width_font.add_argument("--output-path", type=_path)
    h2_text_huffman = h2_commands.add_parser(
        "text-huffman",
        help="verify all context Huffman trees, offsets, codes, and text glyph reachability",
    )
    _add_local_paths(h2_text_huffman)
    h2_text_huffman.add_argument("--output-path", type=_path)
    h2_text_banks = h2_commands.add_parser(
        "text-banks",
        help="verify and decode all 17 context-Huffman text banks without tracking plaintext",
    )
    _add_local_paths(h2_text_banks)
    h2_text_banks.add_argument("--output-path", type=_path)
    h2_unused_technical_assets = h2_commands.add_parser(
        "unused-tech-assets",
        help="verify the unreferenced cloud streams and base palettes retained in the ROM",
    )
    _add_local_paths(h2_unused_technical_assets)
    h2_unused_technical_assets.add_argument("--output-path", type=_path)
    h2_witch_menu_graphics = h2_commands.add_parser(
        "witch-menu-graphics",
        help="verify witch choice palette, bubble frames, pointers, and timer phases",
    )
    _add_local_paths(h2_witch_menu_graphics)
    h2_witch_menu_graphics.add_argument("--output-path", type=_path)
    h2_icon_graphics = h2_commands.add_parser(
        "icon-graphics",
        help="verify the complete icon storage corpus and menu copy/highlight boundaries",
    )
    _add_local_paths(h2_icon_graphics)
    h2_icon_graphics.add_argument("--output-path", type=_path)
    h2_battle_effect_graphics = h2_commands.add_parser(
        "battle-effect-graphics",
        help="decode spell, invocation, status, and battle-transition graphics corpora",
    )
    _add_local_paths(h2_battle_effect_graphics)
    h2_battle_effect_graphics.add_argument("--output-path", type=_path)
    h2_map_tilesets = h2_commands.add_parser(
        "map-tilesets",
        help="decode all map Stack tilesets and verify map/animation usage against ROM",
    )
    _add_local_paths(h2_map_tilesets)
    h2_map_tilesets.add_argument("--output-path", type=_path)
    h2_compression_consumers = h2_commands.add_parser(
        "compression-consumers",
        help="inventory every direct named compression call and its complete corpus owner",
    )
    _add_local_paths(h2_compression_consumers, rom=False)
    h2_compression_consumers.add_argument("--output-path", type=_path)
    h2_map_palettes = h2_commands.add_parser(
        "map-palettes",
        help="verify all map palettes, map-header usage, and effective color-zero behavior",
    )
    _add_local_paths(h2_map_palettes)
    h2_map_palettes.add_argument("--output-path", type=_path)
    h2_map_import = h2_commands.add_parser(
        "map-import", help="build the complete canonical engine-neutral map import"
    )
    _add_local_paths(h2_map_import)
    h2_map_import.add_argument("--output-path", type=_path)
    h2_map_setup = h2_commands.add_parser(
        "map-setup", help="parse map setup selection and verify all six-pointer tables against ROM"
    )
    _add_local_paths(h2_map_setup)
    h2_map_setup.add_argument("--output-path", type=_path)
    h2_map_entities = h2_commands.add_parser(
        "map-entities", help="decode setup entity lists and verify fragment fallthrough against ROM"
    )
    _add_local_paths(h2_map_entities)
    h2_map_entities.add_argument("--output-path", type=_path)
    h2_map_events = h2_commands.add_parser(
        "map-events", help="decode entity, zone, and item event tables against source and ROM"
    )
    _add_local_paths(h2_map_events)
    h2_map_events.add_argument("--output-path", type=_path)
    h2_map_descriptions = h2_commands.add_parser(
        "map-descriptions", help="decode area-description wrappers and payload tables against ROM"
    )
    _add_local_paths(h2_map_descriptions)
    h2_map_descriptions.add_argument("--output-path", type=_path)
    h2_map_init = h2_commands.add_parser(
        "map-init", help="inventory setup initialization entry points and static operation routes"
    )
    _add_local_paths(h2_map_init)
    h2_map_init.add_argument("--output-path", type=_path)
    h2_map_scripts = h2_commands.add_parser(
        "map-scripts",
        help="inventory standalone map setup scripts, labels, commands, and references",
    )
    _add_local_paths(h2_map_scripts, rom=False)
    h2_map_scripts.add_argument("--output-path", type=_path)
    h2_auxiliary_data = h2_commands.add_parser(
        "auxiliary-data", help="inventory graphics, scripting, tech, and sprite-dialogue data"
    )
    _add_local_paths(h2_auxiliary_data, rom=False)
    h2_auxiliary_data.add_argument("--output-path", type=_path)
    h2_sound_data = h2_commands.add_parser(
        "sound-data", help="inventory Z80 music sources and verify bank bytes against the ROM"
    )
    _add_local_paths(h2_sound_data)
    h2_sound_data.add_argument("--output-path", type=_path)

    h3_parser = commands.add_parser("h3", help="run a narrow emulator-backed fixture")
    h3_commands = h3_parser.add_subparsers(dest="h3_command", required=True)
    h3_rng = h3_commands.add_parser("rng", help="verify base and debug-aware RNG behavior")
    h3_rng.add_argument("--rom-path", type=_path, default=DEFAULT_ROM)
    h3_rng.add_argument("--timeout-seconds", type=int, default=60)
    h3_growth = h3_commands.add_parser(
        "growth", help="verify stat-gain and complete level-up behavior"
    )
    _add_local_paths(h3_growth)
    h3_growth.add_argument("--timeout-seconds", type=int, default=60)
    h3_growth_refresh = h3_commands.add_parser(
        "growth-refresh", help="verify level-up derived-stat and equipment refresh behavior"
    )
    _add_local_paths(h3_growth_refresh)
    h3_growth_refresh.add_argument("--timeout-seconds", type=int, default=60)
    h3_growth_prowess = h3_commands.add_parser(
        "growth-prowess", help="verify the HEAL 3 initialization prowess special case"
    )
    _add_local_paths(h3_growth_prowess)
    h3_growth_prowess.add_argument("--timeout-seconds", type=int, default=60)
    h3_stat_clamps = h3_commands.add_parser(
        "stat-clamps", help="verify stat cap and byte-underflow clamp boundaries"
    )
    _add_local_paths(h3_stat_clamps)
    h3_stat_clamps.add_argument("--timeout-seconds", type=int, default=60)
    h3_enemy_curse = h3_commands.add_parser(
        "enemy-curse", help="verify cursed equipment does not mark enemies as cursed"
    )
    _add_local_paths(h3_enemy_curse)
    h3_enemy_curse.add_argument("--timeout-seconds", type=int, default=60)
    h3_battle_exp = h3_commands.add_parser(
        "battle-exp", help="verify natural battle EXP-to-level-up behavior"
    )
    _add_local_paths(h3_battle_exp)
    h3_battle_exp.add_argument("--timeout-seconds", type=int, default=75)
    h3_kill_exp = h3_commands.add_parser(
        "kill-exp", help="verify kill EXP level-difference brackets and promotion offset"
    )
    _add_local_paths(h3_kill_exp)
    h3_kill_exp.add_argument("--timeout-seconds", type=int, default=75)
    h3_award_exp = h3_commands.add_parser(
        "award-exp", help="verify final EXP halving, randomization, and minimum award"
    )
    _add_local_paths(h3_award_exp)
    h3_award_exp.add_argument("--timeout-seconds", type=int, default=90)
    h3_exp_command = h3_commands.add_parser(
        "exp-command", help="verify EXP storage, threshold, and single-level command boundaries"
    )
    _add_local_paths(h3_exp_command)
    h3_exp_command.add_argument("--timeout-seconds", type=int, default=75)
    h3_gold = h3_commands.add_parser(
        "gold", help="verify gold addition, cap, and unsigned-carry boundaries"
    )
    _add_local_paths(h3_gold)
    h3_gold.add_argument("--timeout-seconds", type=int, default=60)
    h3_enemy_drops = h3_commands.add_parser(
        "enemy-drops", help="verify rare, repeated-flag, and guaranteed enemy item drops"
    )
    _add_local_paths(h3_enemy_drops)
    h3_enemy_drops.add_argument("--timeout-seconds", type=int, default=75)
    h3_muddle_confusion = h3_commands.add_parser(
        "muddle-confusion", help="verify the MUDDLE counter/level-2 confusion truth table"
    )
    _add_local_paths(h3_muddle_confusion)
    h3_muddle_confusion.add_argument("--timeout-seconds", type=int, default=75)
    h3_muddle_action_guard = h3_commands.add_parser(
        "muddle-action-guard",
        help="verify muddled ally/enemy protected-target and self-target action guards",
    )
    _add_local_paths(h3_muddle_action_guard)
    h3_muddle_action_guard.add_argument("--timeout-seconds", type=int, default=120)
    h3_battle_ai_action = h3_commands.add_parser(
        "battle-ai-action", help="verify batched battle AI action choice and target tie-breaks"
    )
    _add_local_paths(h3_battle_ai_action)
    h3_battle_ai_action.add_argument("--timeout-seconds", type=int, default=120)
    h3_battlefield_matrix = h3_commands.add_parser(
        "battlefield-matrix", help="verify batched weighted movement and flat-grid boundaries"
    )
    _add_local_paths(h3_battlefield_matrix)
    h3_battlefield_matrix.add_argument("--timeout-seconds", type=int, default=120)
    h3_entity_movement = h3_commands.add_parser(
        "entity-movement", help="verify batched map-entity commands and frame movement"
    )
    _add_local_paths(h3_entity_movement)
    h3_entity_movement.add_argument("--timeout-seconds", type=int, default=120)
    h3_witch_save_actions = h3_commands.add_parser(
        "witch-save-actions",
        help="verify one-launch in-process Save/Load/Copy/Delete and flag-88 routing",
    )
    _add_local_paths(h3_witch_save_actions)
    h3_witch_save_actions.add_argument("--timeout-seconds", type=int, default=120)
    h3_witch_new_game_lifecycle = h3_commands.add_parser(
        "witch-new-game-lifecycle",
        help="verify one-launch witch New action slot, difficulty, save, and MainLoop handoff",
    )
    _add_local_paths(h3_witch_new_game_lifecycle)
    h3_witch_new_game_lifecycle.add_argument("--timeout-seconds", type=int, default=120)
    h3_map_setup_selection = h3_commands.add_parser(
        "map-setup-selection",
        help="verify map setup default, flag, alias, and missing-map selection",
    )
    _add_local_paths(h3_map_setup_selection)
    h3_map_setup_selection.add_argument("--timeout-seconds", type=int, default=120)
    h3_map_init_dispatch = h3_commands.add_parser(
        "map-init-dispatch",
        help="verify missing, active, and direct-return map init dispatch",
    )
    _add_local_paths(h3_map_init_dispatch)
    h3_map_init_dispatch.add_argument("--timeout-seconds", type=int, default=120)
    h3_map_event_dispatch = h3_commands.add_parser(
        "map-event-dispatch",
        help="verify batched entity, zone, and item event first-match dispatch",
    )
    _add_local_paths(h3_map_event_dispatch)
    h3_map_event_dispatch.add_argument("--timeout-seconds", type=int, default=120)
    h3_map_lifecycle = h3_commands.add_parser(
        "map-lifecycle",
        help="verify batched reset, fade-load, reload, and map-load lifecycle boundaries",
    )
    _add_local_paths(h3_map_lifecycle)
    h3_map_lifecycle.add_argument("--timeout-seconds", type=int, default=120)
    h3_map_interaction_trigger = h3_commands.add_parser(
        "map-interaction-trigger",
        help="verify batched roof and step trigger gate, match, and marker boundaries",
    )
    _add_local_paths(h3_map_interaction_trigger)
    h3_map_interaction_trigger.add_argument("--timeout-seconds", type=int, default=120)
    h3_map_animation_vdp = h3_commands.add_parser(
        "map-animation-vdp",
        help="verify batched map-animation counter, wrap, DMA queue, and VRAM behavior",
    )
    _add_local_paths(h3_map_animation_vdp)
    h3_map_animation_vdp.add_argument("--timeout-seconds", type=int, default=120)
    h3_sound_timing = h3_commands.add_parser(
        "sound-timing",
        help="verify batched Z80 music command and live channel-state progression",
    )
    _add_local_paths(h3_sound_timing)
    h3_sound_timing.add_argument("--timeout-seconds", type=int, default=120)
    h3_spell_damage = h3_commands.add_parser(
        "spell-damage", help="verify BLAZE 2 damage across all four fire-resistance settings"
    )
    _add_local_paths(h3_spell_damage)
    h3_spell_damage.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_exp = h3_commands.add_parser(
        "spell-exp", help="verify attack-spell damage EXP brackets, kill bonus, and cap"
    )
    _add_local_paths(h3_spell_exp)
    h3_spell_exp.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_summon = h3_commands.add_parser(
        "spell-summon", help="verify promoted DAO target-count power division"
    )
    _add_local_paths(h3_spell_summon)
    h3_spell_summon.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_healing = h3_commands.add_parser(
        "spell-healing", help="verify HEAL 1 recovery, EXP award, and command replay"
    )
    _add_local_paths(h3_spell_healing)
    h3_spell_healing.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_healing_exp = h3_commands.add_parser(
        "spell-healing-exp", help="verify healing EXP eligibility, minimum, and cap"
    )
    _add_local_paths(h3_spell_healing_exp)
    h3_spell_healing_exp.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_aura = h3_commands.add_parser(
        "spell-aura", help="verify AURA radius targets, all-allies filtering, and EXP accumulation"
    )
    _add_local_paths(h3_spell_aura)
    h3_spell_aura.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_detox = h3_commands.add_parser(
        "spell-detox", help="verify DETOX level masks, ineffective branch, and curse unequip"
    )
    _add_local_paths(h3_spell_detox)
    h3_spell_detox.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_attack = h3_commands.add_parser(
        "spell-attack", help="verify ATTACK 1 fresh application and recast failure"
    )
    _add_local_paths(h3_spell_attack)
    h3_spell_attack.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_muddle = h3_commands.add_parser(
        "spell-muddle", help="verify MUDDLE 2 across all four status-resistance settings"
    )
    _add_local_paths(h3_spell_muddle)
    h3_spell_muddle.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_muddle1 = h3_commands.add_parser(
        "spell-muddle1", help="verify MUDDLE 1 resistance bypass, recast, and level-2 guard"
    )
    _add_local_paths(h3_spell_muddle1)
    h3_spell_muddle1.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_status = h3_commands.add_parser(
        "spell-status", help="verify SLEEP 1 across all four status-resistance settings"
    )
    _add_local_paths(h3_spell_status)
    h3_spell_status.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_desoul = h3_commands.add_parser(
        "spell-desoul", help="verify DESOUL instant death, kill EXP, and gold replay"
    )
    _add_local_paths(h3_spell_desoul)
    h3_spell_desoul.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_mp = h3_commands.add_parser(
        "spell-mp", help="verify SPOIT random MP drain, clamp, EXP, and command replay"
    )
    _add_local_paths(h3_spell_mp)
    h3_spell_mp.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_boost = h3_commands.add_parser(
        "spell-boost", help="verify BOOST 1 fresh application, recast, stats, and replay"
    )
    _add_local_paths(h3_spell_boost)
    h3_spell_boost.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_slow = h3_commands.add_parser(
        "spell-slow", help="verify SLOW 1 resistance thresholds, stats, EXP, and replay"
    )
    _add_local_paths(h3_spell_slow)
    h3_spell_slow.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_dispel = h3_commands.add_parser(
        "spell-dispel",
        help="verify DISPEL 1 spell counting, resistance, recast, EXP, and replay",
    )
    _add_local_paths(h3_spell_dispel)
    h3_spell_dispel.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_silence = h3_commands.add_parser(
        "spell-silence", help="verify SILENCE blocks affected spells before effect dispatch"
    )
    _add_local_paths(h3_spell_silence)
    h3_spell_silence.add_argument("--timeout-seconds", type=int, default=90)
    h3_after_turn = h3_commands.add_parser(
        "after-turn", help="verify status expiry, continuation, and final stat refresh"
    )
    _add_local_paths(h3_after_turn)
    h3_after_turn.add_argument("--timeout-seconds", type=int, default=150)
    return parser


def dispatch(args: argparse.Namespace) -> None:
    if args.command == "verify":
        verify(
            rom_path=args.rom_path,
            upstream_path=args.upstream_path,
            skip_rebuild=args.skip_rebuild,
            skip_extraction=args.skip_extraction,
            skip_runtime=args.skip_runtime,
            full=full_verify_requested(args),
        )
    elif args.command == "init":
        run_powershell("Initialize-LocalResearch.ps1", ("-RomPath", args.rom_path))
    elif args.command == "rom":
        print_record(verify_rom(args.rom_path))
    elif args.command in {"research-index", "index"}:
        if args.index_command == "test":
            upstream = args.upstream_path if args.upstream_path.exists() else None
            print_record(verify_index(upstream))
        else:
            records = query_index(
                query=args.query,
                subsystem=args.subsystem,
                status=args.status,
                fixture=args.fixture,
            )
            result: Any = index_summary(records) if args.summary else index_rows(records)
            if args.json:
                print_json(result)
            elif args.summary:
                print_record(result)
            else:
                _print_rows(result)
    elif args.command == "design-contracts":
        print_record(verify_design_contracts())
    elif args.command == "h2" and args.h2_command == "enemy-gold":
        print_record(
            verify_enemy_gold(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "enemy-drops":
        print_record(
            verify_enemy_item_drops(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-ai":
        print_record(
            verify_battle_ai_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battlefield":
        print_record(
            verify_battlefield_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-loop":
        print_record(
            verify_battle_loop_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-control":
        print_record(
            verify_battle_control_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-actions":
        print_record(
            verify_battle_actions_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-functions":
        print_record(
            verify_battle_functions_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-scene-engine":
        print_record(
            verify_battle_scene_engine_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-scene-animations":
        print_record(
            verify_battle_scene_animation_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-cutscenes":
        print_record(
            verify_battle_cutscene_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "common-scripting":
        print_record(
            verify_scripting_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "common-maps":
        print_record(
            verify_map_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "common-stats":
        print_record(
            verify_stats_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "common-menus":
        print_record(
            verify_menu_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "tech-interrupts":
        print_record(
            verify_interrupt_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "tech-graphics":
        print_record(
            verify_graphics_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "tech-interfaces":
        print_record(
            verify_interface_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "tech-services":
        print_record(
            verify_service_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "gameflow-core":
        print_record(
            verify_gameflow_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "special-screens":
        print_record(
            verify_special_screen_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "remaining-core":
        print_record(
            verify_remaining_core_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-global-data":
        print_record(
            verify_battle_global_data_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "ally-data":
        print_record(
            verify_ally_data_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "core-stats-data":
        print_record(
            verify_core_stats_data_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "item-auxiliary":
        print_record(
            verify_item_auxiliary_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "enemy-map-sprites":
        print_record(
            verify_enemy_map_sprites_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "entity-action-scripts":
        print_record(
            verify_entity_action_script_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-script-engine":
        print_record(
            verify_map_script_engine_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "sprite-dialogue":
        print_record(
            verify_sprite_dialogue_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-cutscene-data":
        print_record(
            verify_battle_cutscene_data_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-spriteset-data":
        print_record(
            verify_battle_spriteset_data_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-routing-data":
        print_record(
            verify_battle_routing_data_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-terrain":
        print_record(
            verify_battle_terrain_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-backgrounds":
        print_record(
            verify_battle_background_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-sprites":
        print_record(
            verify_battle_sprite_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-sprite-animations":
        print_record(
            verify_battle_sprite_animation_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-weapon-ground":
        print_record(
            verify_battle_weapon_ground_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "portraits":
        print_record(
            verify_portrait_graphics_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-data":
        print_record(
            verify_map_data_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-content":
        print_record(
            verify_map_content_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-layouts":
        print_record(
            verify_map_layout_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-sprites":
        print_record(
            verify_map_sprite_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-sprite-assignments":
        print_record(
            verify_map_sprite_assignment_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "special-sprites":
        print_record(
            verify_special_sprite_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "special-screen-graphics":
        print_record(
            verify_special_screen_graphics_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "special-screen-presentation":
        print_record(
            verify_special_screen_presentation_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "ui-graphics":
        print_record(
            verify_ui_graphics_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "ui-layouts":
        print_record(
            verify_ui_layout_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "variable-width-font":
        print_record(
            verify_variable_width_font_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "text-huffman":
        print_record(
            verify_text_huffman_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "text-banks":
        print_record(
            verify_text_banks_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "unused-tech-assets":
        print_record(
            verify_unused_technical_assets_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "witch-menu-graphics":
        print_record(
            verify_witch_menu_graphics_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "icon-graphics":
        print_record(
            verify_icon_graphics_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-effect-graphics":
        print_record(
            verify_battle_effect_graphics_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-tilesets":
        print_record(
            verify_map_tileset_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "compression-consumers":
        print_record(
            verify_compression_consumer_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-palettes":
        print_record(
            verify_map_palette_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-import":
        print_record(
            verify_canonical_map_import(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-setup":
        print_record(
            verify_map_setup_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-entities":
        print_record(
            verify_map_entities_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-events":
        print_record(
            verify_map_events_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-descriptions":
        print_record(
            verify_map_descriptions_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-init":
        print_record(
            verify_map_init_contract(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "map-scripts":
        print_record(
            verify_map_scripts_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "auxiliary-data":
        print_record(
            verify_auxiliary_data_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "sound-data":
        print_record(
            verify_sound_data_inventory(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h3" and args.h3_command == "rng":
        print_record(verify_rng(args.rom_path, timeout_seconds=args.timeout_seconds))
    elif args.command == "h3" and args.h3_command == "growth":
        print_record(
            verify_growth(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "growth-refresh":
        print_record(
            verify_level_up_refresh(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "growth-prowess":
        print_record(
            verify_initialization_prowess(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "stat-clamps":
        print_record(
            verify_stat_clamp_boundaries(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "enemy-curse":
        print_record(
            verify_enemy_curse_suppression(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "battle-exp":
        print_record(
            verify_battle_exp_level_up(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "kill-exp":
        print_record(
            verify_kill_exp_level_differences(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "award-exp":
        print_record(
            verify_award_exp_randomization(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "exp-command":
        print_record(
            verify_exp_command_boundaries(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "gold":
        print_record(
            verify_gold_boundaries(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "enemy-drops":
        print_record(
            verify_enemy_item_drop_behavior(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "muddle-confusion":
        print_record(
            verify_muddle_confusion(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "muddle-action-guard":
        print_record(
            verify_muddle_action_guard(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "battle-ai-action":
        print_record(
            verify_battle_ai_action_choice(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "battlefield-matrix":
        print_record(
            verify_battlefield_movement_matrix(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "entity-movement":
        print_record(
            verify_entity_movement_matrix(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "witch-save-actions":
        print_record(
            verify_witch_save_actions(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "witch-new-game-lifecycle":
        print_record(
            verify_witch_new_game_lifecycle(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "map-setup-selection":
        print_record(
            verify_map_setup_selection(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "map-init-dispatch":
        print_record(
            verify_map_init_dispatch(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "map-event-dispatch":
        print_record(
            verify_map_event_dispatch(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "map-lifecycle":
        print_record(
            verify_map_lifecycle(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "map-interaction-trigger":
        print_record(
            verify_map_interaction_trigger(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "map-animation-vdp":
        print_record(
            verify_map_animation_vdp(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "sound-timing":
        print_record(
            verify_sound_timing(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-damage":
        print_record(
            verify_spell_damage(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-exp":
        print_record(
            verify_spell_damage_exp(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-summon":
        print_record(
            verify_spell_summon(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-healing":
        print_record(
            verify_spell_healing(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-healing-exp":
        print_record(
            verify_spell_healing_exp(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-aura":
        print_record(
            verify_spell_aura(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-detox":
        print_record(
            verify_spell_detox(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-attack":
        print_record(
            verify_spell_attack(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-muddle":
        print_record(
            verify_spell_muddle(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-muddle1":
        print_record(
            verify_spell_muddle1(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-status":
        print_record(
            verify_spell_status(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-desoul":
        print_record(
            verify_spell_desoul(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-mp":
        print_record(
            verify_spell_mp_absorb(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-boost":
        print_record(
            verify_spell_boost(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-slow":
        print_record(
            verify_spell_slow(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-dispel":
        print_record(
            verify_spell_dispel(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-silence":
        print_record(
            verify_spell_silence_gate(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "after-turn":
        print_record(
            verify_after_turn_status_lifecycle(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    else:
        raise AssertionError(f"unhandled command: {args.command}")


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    try:
        dispatch(parser.parse_args(argv))
    except (OSError, ValueError, RuntimeError, subprocess.CalledProcessError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0
