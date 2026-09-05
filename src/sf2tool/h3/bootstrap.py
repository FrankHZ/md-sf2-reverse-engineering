"""Closed bootstrap ownership for every tracked H3 BizHawk launcher."""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path

from sf2tool.paths import repo_path


@dataclass(frozen=True)
class BootstrapProfile:
    """One concrete original-runtime session setup, not an evidence classification."""

    name: str
    battle01_compatible: bool
    isolation_reason: str


@dataclass(frozen=True)
class ObserverLaunch:
    """One observer and its exact number of BizHawk launches per CLI invocation."""

    observer: str
    expected_launches: int = 1
    cases_fixture: str | None = None


@dataclass(frozen=True)
class CommandLaunch:
    """The concrete CLI dispatch owner and complete observer launch plan."""

    dispatch_module: str
    dispatch_function: str
    profile: str
    launches: tuple[ObserverLaunch, ...]

    @property
    def observers(self) -> tuple[str, ...]:
        return tuple(dict.fromkeys(launch.observer for launch in self.launches))

    @property
    def expected_launches(self) -> int:
        return sum(launch.expected_launches for launch in self.launches)


PROFILES = {
    "battle01-intro-skip": BootstrapProfile(
        name="battle01-intro-skip",
        battle01_compatible=True,
        isolation_reason=(
            "Battle Test prompt 1 selects Battle 01; prompt 2 selects nonzero option 1."
        ),
    ),
    "map-debug-host": BootstrapProfile(
        name="map-debug-host",
        battle01_compatible=False,
        isolation_reason=(
            "Map-handler fixtures use the debug host only to reach their controlled map setup "
            "seam; "
            "they must not initialize Battle 01."
        ),
    ),
    "direct-function-seam": BootstrapProfile(
        name="direct-function-seam",
        battle01_compatible=False,
        isolation_reason=(
            "Growth and clamp fixtures enter a source-defined function seam directly, so "
            "Battle Test "
            "state would be unrelated setup."
        ),
    ),
    "witch-menu": BootstrapProfile(
        name="witch-menu",
        battle01_compatible=False,
        isolation_reason=(
            "Witch Save/New fixtures must traverse the original title and witch-menu state "
            "machine, "
            "not the debug Battle Test."
        ),
    ),
    "sound-driver": BootstrapProfile(
        name="sound-driver",
        battle01_compatible=False,
        isolation_reason=(
            "The Z80 music fixture observes boot-frame driver state and music commands without a "
            "Battle Test host."
        ),
    ),
    "original-reference": BootstrapProfile(
        name="original-reference",
        battle01_compatible=False,
        isolation_reason=(
            "Original-reference transport and scenario-API preflight use declared power-on "
            "contracts and do not use an existing H3 bootstrap seam."
        ),
    ),
}

BATTLE01_OBSERVERS = (
    "tools/bizhawk/after_turn_status_lifecycle_observer.lua",
    "tools/bizhawk/award_exp_randomization_observer.lua",
    "tools/bizhawk/battle_ai_action_choice_observer.lua",
    "tools/bizhawk/battle_exp_level_up_observer.lua",
    "tools/bizhawk/battlefield_movement_matrix_observer.lua",
    "tools/bizhawk/enemy_curse_suppression_observer.lua",
    "tools/bizhawk/enemy_item_drop_behavior_observer.lua",
    "tools/bizhawk/exp_command_boundaries_observer.lua",
    "tools/bizhawk/gold_boundaries_observer.lua",
    "tools/bizhawk/kill_exp_level_difference_observer.lua",
    "tools/bizhawk/muddle_action_guard_observer.lua",
    "tools/bizhawk/muddle_confusion_observer.lua",
    "tools/bizhawk/random_services_observer.lua",
    "tools/bizhawk/rng_observer.lua",
    "tools/bizhawk/spell_attack_observer.lua",
    "tools/bizhawk/spell_aura_targets_observer.lua",
    "tools/bizhawk/spell_boost_observer.lua",
    "tools/bizhawk/spell_damage_exp_observer.lua",
    "tools/bizhawk/spell_damage_resistance_observer.lua",
    "tools/bizhawk/spell_desoul_observer.lua",
    "tools/bizhawk/spell_detox_observer.lua",
    "tools/bizhawk/spell_dispel_observer.lua",
    "tools/bizhawk/spell_healing_exp_boundaries_observer.lua",
    "tools/bizhawk/spell_healing_observer.lua",
    "tools/bizhawk/spell_mp_absorb_observer.lua",
    "tools/bizhawk/spell_silence_gate_observer.lua",
    "tools/bizhawk/spell_slow_observer.lua",
    "tools/bizhawk/spell_status_observer.lua",
)

MAP_DEBUG_HOST_OBSERVERS = (
    "tools/bizhawk/entity_movement_matrix_observer.lua",
    "tools/bizhawk/entity_population_reload_observer.lua",
    "tools/bizhawk/force_state_active_party_observer.lua",
    "tools/bizhawk/force_state_roster_death_observer.lua",
    "tools/bizhawk/map_animation_vdp_observer.lua",
    "tools/bizhawk/map_block_copy_lifecycle_observer.lua",
    "tools/bizhawk/map_block_mutation_observer.lua",
    "tools/bizhawk/map_camera_control_observer.lua",
    "tools/bizhawk/map_entity_action_bridge_observer.lua",
    "tools/bizhawk/map_entity_gesture_relationship_motion_observer.lua",
    "tools/bizhawk/map_entity_lifecycle_presentation_observer.lua",
    "tools/bizhawk/map_entity_placement_observer.lua",
    "tools/bizhawk/map_event_dispatch_observer.lua",
    "tools/bizhawk/map_init_dispatch_observer.lua",
    "tools/bizhawk/map_interaction_trigger_observer.lua",
    "tools/bizhawk/map_lifecycle_observer.lua",
    "tools/bizhawk/map_script_control_audio_observer.lua",
    "tools/bizhawk/map_script_dialogue_observer.lua",
    "tools/bizhawk/map_script_entity_clone_observer.lua",
    "tools/bizhawk/map_script_entity_presentation_fx_observer.lua",
    "tools/bizhawk/map_script_screen_presentation_observer.lua",
    "tools/bizhawk/map_script_transition_observer.lua",
    "tools/bizhawk/map_script_ui_primary_observer.lua",
    "tools/bizhawk/map_setup_selection_observer.lua",
    "tools/bizhawk/story_state_observer.lua",
)

DIRECT_FUNCTION_SEAM_OBSERVERS = (
    "tools/bizhawk/ally_initialization_prowess_observer.lua",
    "tools/bizhawk/blacksmith_mithril_observer.lua",
    "tools/bizhawk/church_raise_lifecycle_observer.lua",
    "tools/bizhawk/church_cure_lifecycle_observer.lua",
    "tools/bizhawk/church_save_lifecycle_observer.lua",
    "tools/bizhawk/service_menu_entry_return_observer.lua",
    "tools/bizhawk/controller_input_observer.lua",
    "tools/bizhawk/level_up_boundaries_observer.lua",
    "tools/bizhawk/level_up_observer.lua",
    "tools/bizhawk/level_up_refresh_observer.lua",
    "tools/bizhawk/stat_clamp_boundaries_observer.lua",
    "tools/bizhawk/stat_gain_observer.lua",
    "tools/bizhawk/sram_lifecycle_observer.lua",
)

WITCH_MENU_OBSERVERS = (
    "tools/bizhawk/map3_admitted_start_observer.lua",
    "tools/bizhawk/map3_battle01_natural_route_observer.lua",
    "tools/bizhawk/map3_messenger_acceptance_observer.lua",
    "tools/bizhawk/map3_original_player_locomotion_animation_observer.lua",
    "tools/bizhawk/witch_new_game_lifecycle_observer.lua",
    "tools/bizhawk/witch_save_actions_observer.lua",
    "tools/bizhawk/witch_save_menu_actions_observer.lua",
)

SOUND_DRIVER_OBSERVERS = ("tools/bizhawk/sound_timing_observer.lua",)
ORIGINAL_REFERENCE_OBSERVERS = (
    "tools/bizhawk/original_reference_replay_observer.lua",
    "tools/bizhawk/original_reference_scenario_observer.lua",
)


def _profiles(profile: str, paths: tuple[str, ...]) -> dict[str, str]:
    return {path: profile for path in paths}


OBSERVER_PROFILES = (
    _profiles("battle01-intro-skip", BATTLE01_OBSERVERS)
    | _profiles("map-debug-host", MAP_DEBUG_HOST_OBSERVERS)
    | _profiles("direct-function-seam", DIRECT_FUNCTION_SEAM_OBSERVERS)
    | _profiles("witch-menu", WITCH_MENU_OBSERVERS)
    | _profiles("sound-driver", SOUND_DRIVER_OBSERVERS)
    | _profiles("original-reference", ORIGINAL_REFERENCE_OBSERVERS)
)

def _launch(
    observer: str, expected_launches: int = 1, *, cases_fixture: str | None = None
) -> ObserverLaunch:
    return ObserverLaunch(observer, expected_launches, cases_fixture)


def _command(
    module: str,
    function: str,
    profile: str,
    *launches: ObserverLaunch,
) -> CommandLaunch:
    return CommandLaunch(module, function, profile, launches)


COMMAND_LAUNCHES = {
    "rng": _command(
        "sf2tool.h3.rng", "verify_rng", "battle01-intro-skip",
        _launch("tools/bizhawk/rng_observer.lua", 2),
    ),
    "random-services": _command(
        "sf2tool.h3.random_services", "verify_random_services", "battle01-intro-skip",
        _launch("tools/bizhawk/random_services_observer.lua"),
    ),
    "sram-lifecycle": _command(
        "sf2tool.h3.sram_lifecycle", "verify_sram_lifecycle", "direct-function-seam",
        _launch("tools/bizhawk/sram_lifecycle_observer.lua"),
    ),
    "blacksmith-mithril": _command(
        "sf2tool.h3.blacksmith_mithril", "verify_blacksmith_mithril", "direct-function-seam",
        _launch("tools/bizhawk/blacksmith_mithril_observer.lua"),
    ),
    "church-raise-lifecycle": _command(
        "sf2tool.h3.church_raise_lifecycle",
        "verify_church_raise_lifecycle",
        "direct-function-seam",
        _launch("tools/bizhawk/church_raise_lifecycle_observer.lua"),
    ),
    "church-cure-lifecycle": _command(
        "sf2tool.h3.church_cure_lifecycle",
        "verify_church_cure_lifecycle",
        "direct-function-seam",
        _launch("tools/bizhawk/church_cure_lifecycle_observer.lua"),
    ),
    "church-save-lifecycle": _command(
        "sf2tool.h3.church_save_lifecycle",
        "verify_church_save_lifecycle",
        "direct-function-seam",
        _launch("tools/bizhawk/church_save_lifecycle_observer.lua"),
    ),
    "service-menu-lifecycle": _command(
        "sf2tool.h3.service_menu_lifecycle",
        "verify_service_menu_lifecycle",
        "direct-function-seam",
        _launch("tools/bizhawk/service_menu_entry_return_observer.lua"),
    ),
    "controller-input": _command(
        "sf2tool.h3.controller_input", "verify_controller_input", "direct-function-seam",
        _launch("tools/bizhawk/controller_input_observer.lua"),
    ),
    "growth": _command(
        "sf2tool.h3.growth", "verify_growth", "direct-function-seam",
        _launch("tools/bizhawk/stat_gain_observer.lua"),
        _launch("tools/bizhawk/level_up_observer.lua"),
        _launch("tools/bizhawk/level_up_boundaries_observer.lua"),
        _launch(
            "tools/bizhawk/level_up_refresh_observer.lua", 8,
            cases_fixture="tests/fixtures/h3/level-up-refresh-v1.json",
        ),
        _launch(
            "tools/bizhawk/ally_initialization_prowess_observer.lua", 16,
            cases_fixture="tests/fixtures/h3/ally-initialization-prowess-v1.json",
        ),
    ),
    "growth-refresh": _command(
        "sf2tool.h3.growth", "verify_level_up_refresh", "direct-function-seam",
        _launch(
            "tools/bizhawk/level_up_refresh_observer.lua", 8,
            cases_fixture="tests/fixtures/h3/level-up-refresh-v1.json",
        ),
    ),
    "growth-prowess": _command(
        "sf2tool.h3.growth", "verify_initialization_prowess", "direct-function-seam",
        _launch(
            "tools/bizhawk/ally_initialization_prowess_observer.lua", 16,
            cases_fixture="tests/fixtures/h3/ally-initialization-prowess-v1.json",
        ),
    ),
    "stat-clamps": _command(
        "sf2tool.h3.stat_clamps", "verify_stat_clamp_boundaries", "direct-function-seam",
        _launch("tools/bizhawk/stat_clamp_boundaries_observer.lua"),
    ),
    "enemy-curse": _command(
        "sf2tool.h3.enemy_curse", "verify_enemy_curse_suppression", "battle01-intro-skip",
        _launch("tools/bizhawk/enemy_curse_suppression_observer.lua"),
    ),
    "battle-exp": _command(
        "sf2tool.h3.battle_exp", "verify_battle_exp_level_up", "battle01-intro-skip",
        _launch("tools/bizhawk/battle_exp_level_up_observer.lua"),
    ),
    "kill-exp": _command(
        "sf2tool.h3.kill_exp", "verify_kill_exp_level_differences", "battle01-intro-skip",
        _launch("tools/bizhawk/kill_exp_level_difference_observer.lua"),
    ),
    "award-exp": _command(
        "sf2tool.h3.award_exp", "verify_award_exp_randomization", "battle01-intro-skip",
        _launch("tools/bizhawk/award_exp_randomization_observer.lua"),
    ),
    "exp-command": _command(
        "sf2tool.h3.exp_command", "verify_exp_command_boundaries", "battle01-intro-skip",
        _launch("tools/bizhawk/exp_command_boundaries_observer.lua"),
    ),
    "gold": _command(
        "sf2tool.h3.gold", "verify_gold_boundaries", "battle01-intro-skip",
        _launch("tools/bizhawk/gold_boundaries_observer.lua"),
    ),
    "enemy-drops": _command(
        "sf2tool.h3.enemy_drops", "verify_enemy_item_drop_behavior", "battle01-intro-skip",
        _launch("tools/bizhawk/enemy_item_drop_behavior_observer.lua"),
    ),
    "muddle-confusion": _command(
        "sf2tool.h3.muddle_confusion", "verify_muddle_confusion", "battle01-intro-skip",
        _launch("tools/bizhawk/muddle_confusion_observer.lua"),
    ),
    "muddle-action-guard": _command(
        "sf2tool.h3.muddle_action_guard", "verify_muddle_action_guard", "battle01-intro-skip",
        _launch("tools/bizhawk/muddle_action_guard_observer.lua"),
    ),
    "battle-ai-action": _command(
        "sf2tool.h3.battle_ai_action", "verify_battle_ai_action_choice", "battle01-intro-skip",
        _launch("tools/bizhawk/battle_ai_action_choice_observer.lua"),
    ),
    "battlefield-matrix": _command(
        "sf2tool.h3.battlefield_matrix",
        "verify_battlefield_movement_matrix",
        "battle01-intro-skip",
        _launch("tools/bizhawk/battlefield_movement_matrix_observer.lua"),
    ),
    "entity-movement": _command(
        "sf2tool.h3.entity_movement", "verify_entity_movement_matrix", "map-debug-host",
        _launch("tools/bizhawk/entity_movement_matrix_observer.lua"),
    ),
    "witch-save-actions": _command(
        "sf2tool.h3.witch_save_actions", "verify_witch_save_actions", "witch-menu",
        _launch("tools/bizhawk/witch_save_actions_observer.lua"),
    ),
    "witch-save-menu-actions": _command(
        "sf2tool.h3.witch_save_menu_actions",
        "verify_witch_save_menu_actions",
        "witch-menu",
        _launch("tools/bizhawk/witch_save_menu_actions_observer.lua"),
    ),
    "witch-new-game-lifecycle": _command(
        "sf2tool.h3.witch_new_game_lifecycle", "verify_witch_new_game_lifecycle", "witch-menu",
        _launch("tools/bizhawk/witch_new_game_lifecycle_observer.lua"),
    ),
    "map3-admitted-start": _command(
        "sf2tool.h3.map3_admitted_start",
        "verify_map3_admitted_start",
        "witch-menu",
        _launch(
            "tools/bizhawk/map3_admitted_start_observer.lua",
            cases_fixture="tests/fixtures/h3/map3-admitted-start-v1.json",
        ),
    ),
    "map3-original-player-locomotion-animation": _command(
        "sf2tool.h3.map3_original_player_locomotion_animation",
        "verify_map3_original_player_locomotion_animation",
        "witch-menu",
        _launch("tools/bizhawk/map3_original_player_locomotion_animation_observer.lua"),
    ),
    "map3-battle01-natural-route": _command(
        "sf2tool.h3.map3_battle01_natural_route",
        "verify_map3_battle01_natural_route",
        "witch-menu",
        _launch(
            "tools/bizhawk/map3_battle01_natural_route_observer.lua",
            cases_fixture="tests/fixtures/h3/map3-battle01-natural-route-v1.json",
        ),
    ),
    "map3-battle01-player-ready": _command(
        "sf2tool.h3.map3_battle01_player_ready",
        "verify_map3_battle01_player_ready",
        "witch-menu",
        _launch(
            "tools/bizhawk/map3_messenger_acceptance_observer.lua",
            cases_fixture="tests/fixtures/h3/map3-battle01-player-ready-v1.json",
        ),
    ),
    "map3-messenger-acceptance": _command(
        "sf2tool.h3.map3_messenger_acceptance",
        "verify_map3_messenger_acceptance",
        "witch-menu",
        _launch(
            "tools/bizhawk/map3_messenger_acceptance_observer.lua",
            cases_fixture="tests/fixtures/h3/map3-messenger-acceptance-v1.json",
        ),
    ),
    "map-setup-selection": _command(
        "sf2tool.h3.map_setup_selection", "verify_map_setup_selection", "map-debug-host",
        _launch("tools/bizhawk/map_setup_selection_observer.lua"),
    ),
    "map-init-dispatch": _command(
        "sf2tool.h3.map_init_dispatch", "verify_map_init_dispatch", "map-debug-host",
        _launch("tools/bizhawk/map_init_dispatch_observer.lua"),
    ),
    "map-event-dispatch": _command(
        "sf2tool.h3.map_event_dispatch", "verify_map_event_dispatch", "map-debug-host",
        _launch("tools/bizhawk/map_event_dispatch_observer.lua"),
    ),
    "map-lifecycle": _command(
        "sf2tool.h3.map_lifecycle", "verify_map_lifecycle", "map-debug-host",
        _launch("tools/bizhawk/map_lifecycle_observer.lua"),
    ),
    "map-script-control-audio": _command(
        "sf2tool.h3.map_script_control_audio", "verify_map_script_control_audio", "map-debug-host",
        _launch("tools/bizhawk/map_script_control_audio_observer.lua"),
    ),
    "map-script-transition": _command(
        "sf2tool.h3.map_script_transition", "verify_map_script_transition", "map-debug-host",
        _launch("tools/bizhawk/map_script_transition_observer.lua"),
    ),
    "map-block-mutation": _command(
        "sf2tool.h3.map_block_mutation", "verify_map_block_mutation", "map-debug-host",
        _launch("tools/bizhawk/map_block_mutation_observer.lua"),
    ),
    "map-block-copy-lifecycle": _command(
        "sf2tool.h3.map_block_copy_lifecycle",
        "verify_map_block_copy_lifecycle",
        "map-debug-host",
        _launch("tools/bizhawk/map_block_copy_lifecycle_observer.lua"),
    ),
    "entity-population-reload": _command(
        "sf2tool.h3.entity_population_reload", "verify_entity_population_reload", "map-debug-host",
        _launch("tools/bizhawk/entity_population_reload_observer.lua"),
    ),
    "map-interaction-trigger": _command(
        "sf2tool.h3.map_interaction_trigger", "verify_map_interaction_trigger", "map-debug-host",
        _launch("tools/bizhawk/map_interaction_trigger_observer.lua"),
    ),
    "map-camera-control": _command(
        "sf2tool.h3.map_camera_control", "verify_map_camera_control", "map-debug-host",
        _launch("tools/bizhawk/map_camera_control_observer.lua"),
    ),
    "map-entity-placement": _command(
        "sf2tool.h3.map_entity_placement", "verify_map_entity_placement", "map-debug-host",
        _launch("tools/bizhawk/map_entity_placement_observer.lua"),
    ),
    "map-script-ui-primary": _command(
        "sf2tool.h3.map_script_ui_primary", "verify_map_script_ui_primary", "map-debug-host",
        _launch("tools/bizhawk/map_script_ui_primary_observer.lua"),
    ),
    "map-script-entity-presentation-fx": _command(
        "sf2tool.h3.map_script_entity_presentation_fx",
        "verify_map_script_entity_presentation_fx",
        "map-debug-host",
        _launch("tools/bizhawk/map_script_entity_presentation_fx_observer.lua"),
    ),
    "map-script-entity-clone": _command(
        "sf2tool.h3.map_script_entity_clone", "verify_map_script_entity_clone", "map-debug-host",
        _launch("tools/bizhawk/map_script_entity_clone_observer.lua"),
    ),
    "map-script-dialogue": _command(
        "sf2tool.h3.map_script_dialogue", "verify_map_script_dialogue", "map-debug-host",
        _launch("tools/bizhawk/map_script_dialogue_observer.lua"),
    ),
    "map-script-screen-presentation": _command(
        "sf2tool.h3.map_script_screen_presentation",
        "verify_map_script_screen_presentation",
        "map-debug-host",
        _launch("tools/bizhawk/map_script_screen_presentation_observer.lua"),
    ),
    "map-entity-lifecycle-presentation": _command(
        "sf2tool.h3.map_entity_lifecycle_presentation",
        "verify_map_entity_lifecycle_presentation",
        "map-debug-host",
        _launch("tools/bizhawk/map_entity_lifecycle_presentation_observer.lua"),
    ),
    "map-entity-gesture-relationship-motion": _command(
        "sf2tool.h3.map_entity_gesture_relationship_motion",
        "verify_map_entity_gesture_relationship_motion",
        "map-debug-host",
        _launch("tools/bizhawk/map_entity_gesture_relationship_motion_observer.lua"),
    ),
    "map-entity-action-bridge": _command(
        "sf2tool.h3.map_entity_action_bridge", "verify_map_entity_action_bridge", "map-debug-host",
        _launch("tools/bizhawk/map_entity_action_bridge_observer.lua"),
    ),
    "map-animation-vdp": _command(
        "sf2tool.h3.map_animation_vdp", "verify_map_animation_vdp", "map-debug-host",
        _launch("tools/bizhawk/map_animation_vdp_observer.lua"),
    ),
    "sound-timing": _command(
        "sf2tool.h3.sound_timing", "verify_sound_timing", "sound-driver",
        _launch("tools/bizhawk/sound_timing_observer.lua"),
    ),
    "story-state": _command(
        "sf2tool.h3.story_state", "verify_story_state", "map-debug-host",
        _launch("tools/bizhawk/story_state_observer.lua"),
    ),
    "force-state-active-party": _command(
        "sf2tool.h3.force_state_active_party", "verify_force_state_active_party", "map-debug-host",
        _launch("tools/bizhawk/force_state_active_party_observer.lua"),
    ),
    "force-state-roster-death": _command(
        "sf2tool.h3.force_state_roster_death",
        "verify_force_state_roster_death",
        "map-debug-host",
        _launch(
            "tools/bizhawk/force_state_roster_death_observer.lua",
            14,
            cases_fixture="tests/fixtures/h3/force-state-roster-death-v1.json",
        ),
    ),
    "spell-damage": _command(
        "sf2tool.h3.spell_damage", "verify_spell_damage", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_damage_resistance_observer.lua"),
    ),
    "spell-exp": _command(
        "sf2tool.h3.spell_exp", "verify_spell_damage_exp", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_damage_exp_observer.lua"),
    ),
    "spell-summon": _command(
        "sf2tool.h3.spell_damage", "verify_spell_summon", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_damage_resistance_observer.lua"),
    ),
    "spell-healing": _command(
        "sf2tool.h3.spell_healing", "verify_spell_healing", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_healing_observer.lua"),
    ),
    "spell-healing-exp": _command(
        "sf2tool.h3.spell_healing", "verify_spell_healing_exp", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_healing_exp_boundaries_observer.lua"),
    ),
    "spell-aura": _command(
        "sf2tool.h3.spell_healing", "verify_spell_aura", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_aura_targets_observer.lua"),
    ),
    "spell-detox": _command(
        "sf2tool.h3.spell_detox", "verify_spell_detox", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_detox_observer.lua"),
    ),
    "spell-attack": _command(
        "sf2tool.h3.spell_attack", "verify_spell_attack", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_attack_observer.lua"),
    ),
    "spell-muddle": _command(
        "sf2tool.h3.spell_muddle", "verify_spell_muddle", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_status_observer.lua"),
    ),
    "spell-muddle1": _command(
        "sf2tool.h3.spell_muddle", "verify_spell_muddle1", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_status_observer.lua"),
    ),
    "spell-status": _command(
        "sf2tool.h3.spell_status", "verify_spell_status", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_status_observer.lua"),
    ),
    "spell-desoul": _command(
        "sf2tool.h3.spell_desoul", "verify_spell_desoul", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_desoul_observer.lua"),
    ),
    "spell-mp": _command(
        "sf2tool.h3.spell_mp", "verify_spell_mp_absorb", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_mp_absorb_observer.lua"),
    ),
    "spell-boost": _command(
        "sf2tool.h3.spell_boost", "verify_spell_boost", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_boost_observer.lua"),
    ),
    "spell-slow": _command(
        "sf2tool.h3.spell_slow", "verify_spell_slow", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_slow_observer.lua"),
    ),
    "spell-dispel": _command(
        "sf2tool.h3.spell_dispel", "verify_spell_dispel", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_dispel_observer.lua"),
    ),
    "spell-silence": _command(
        "sf2tool.h3.spell_silence", "verify_spell_silence_gate", "battle01-intro-skip",
        _launch("tools/bizhawk/spell_silence_gate_observer.lua"),
    ),
    "after-turn": _command(
        "sf2tool.h3.after_turn", "verify_after_turn_status_lifecycle", "battle01-intro-skip",
        _launch("tools/bizhawk/after_turn_status_lifecycle_observer.lua"),
    ),
    "original-reference-replay-capability": _command(
        "sf2tool.h3.original_reference_replay",
        "run_original_reference_replay",
        "original-reference",
        _launch("tools/bizhawk/original_reference_replay_observer.lua"),
    ),
    "original-reference-replay-scenario-api": _command(
        "sf2tool.h3.original_reference_scenario",
        "run_original_reference_scenario",
        "original-reference",
        _launch("tools/bizhawk/original_reference_scenario_observer.lua", 0),
    ),
}

BATTLE01_COMMANDS = tuple(
    command for command, launch in COMMAND_LAUNCHES.items()
    if launch.profile == "battle01-intro-skip"
)
MAP_DEBUG_HOST_COMMANDS = tuple(
    command for command, launch in COMMAND_LAUNCHES.items()
    if launch.profile == "map-debug-host"
)
DIRECT_FUNCTION_SEAM_COMMANDS = tuple(
    command for command, launch in COMMAND_LAUNCHES.items()
    if launch.profile == "direct-function-seam"
)
WITCH_MENU_COMMANDS = tuple(
    command for command, launch in COMMAND_LAUNCHES.items()
    if launch.profile == "witch-menu"
)
SOUND_DRIVER_COMMANDS = tuple(
    command for command, launch in COMMAND_LAUNCHES.items()
    if launch.profile == "sound-driver"
)
ORIGINAL_REFERENCE_COMMANDS = tuple(
    command for command, launch in COMMAND_LAUNCHES.items()
    if launch.profile == "original-reference"
)
H3_COMMAND_PROFILES = {
    command: launch.profile for command, launch in COMMAND_LAUNCHES.items()
}

LEGACY_H3_LAUNCHERS = (
    "scripts/Test-H3AttackChainFixture.ps1",
    "scripts/Test-H3Battle01RegionActivationFixture.ps1",
    "scripts/Test-H3Battle01SecondaryActivationFixture.ps1",
    "scripts/Test-H3Battle01TurnOrderFixture.ps1",
    "scripts/Test-H3CounterBurstRockFixture.ps1",
    "scripts/Test-H3CounterRangeFixture.ps1",
    "scripts/Test-H3CounterSameSideFixture.ps1",
    "scripts/Test-H3CounterSleepFixture.ps1",
    "scripts/Test-H3CounterSpecialEnemiesFixture.ps1",
    "scripts/Test-H3CounterStunFixture.ps1",
    "scripts/Test-H3DodgeFixture.ps1",
    "scripts/Test-H3DoubleValidationFixture.ps1",
    "scripts/Test-H3LethalFollowupFixture.ps1",
    "scripts/Test-H3PhysicalDamageFixture.ps1",
    "scripts/Test-H3TurnOrderBoundariesFixture.ps1",
)

LEGACY_LAUNCHER_PROFILES = _profiles("battle01-intro-skip", LEGACY_H3_LAUNCHERS)
BOOTSTRAP_LIBRARY = repo_path("tools/bizhawk/bootstrap.lua")


def observer_profile(observer_path: Path) -> BootstrapProfile:
    """Return the closed profile for a tracked observer, rejecting unregistered launchers."""

    try:
        relative = (
            observer_path.resolve(strict=True).relative_to(repo_path(".").resolve()).as_posix()
        )
    except ValueError as error:
        raise ValueError(
            f"H3 observer is outside the tracked repository: {observer_path}"
        ) from error
    try:
        return PROFILES[OBSERVER_PROFILES[relative]]
    except KeyError as error:
        raise ValueError(f"H3 observer has no bootstrap profile: {relative}") from error


def runtime_bootstrap(observer_path: Path) -> dict[str, object]:
    """Configuration supplied unchanged to tracked Lua templates."""

    profile = observer_profile(observer_path)
    return {
        "profile": profile.name,
        "isolationReason": profile.isolation_reason,
    }
