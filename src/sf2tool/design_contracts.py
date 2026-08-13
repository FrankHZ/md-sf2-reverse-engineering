from __future__ import annotations

from typing import Any

from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path

CONTRACTS = {
    "docs/design/contracts/ally-definition-data.md": {
        "tests/fixtures/h2/ally-data-static-v1.json": (
            "sf2-ally-data-static-v1"
        ),
        "tests/fixtures/h2/map-sprite-assignments-static-v1.json": (
            "sf2-map-sprite-assignments-static-v1"
        ),
    },
    "docs/design/contracts/audio-system.md": {
        "tests/fixtures/h2/gameflow-core-static-v1.json": (
            "sf2-gameflow-core-static-v1"
        ),
        "tests/fixtures/h2/sound-data-static-v1.json": (
            "sf2-sound-data-static-v1"
        ),
        "tests/fixtures/h3/sound-timing-v1.json": (
            "sf2-sound-timing-runtime-v1"
        ),
    },
    "docs/design/contracts/battle-ai-decision.md": {
        "tests/fixtures/h2/battle-ai-static-v1.json": "sf2-battle-ai-static-v1",
        "tests/fixtures/h2/battle-ai-priority-static-v1.json": (
            "sf2-battle-ai-priority-static-v1"
        ),
        "tests/fixtures/h2/battle-ai-healing-static-v1.json": (
            "sf2-battle-ai-healing-static-v1"
        ),
        "tests/fixtures/h2/battle-ai-support-static-v1.json": (
            "sf2-battle-ai-support-static-v1"
        ),
        "tests/fixtures/h2/battle-ai-action-choice-static-v1.json": (
            "sf2-battle-ai-action-choice-static-v1"
        ),
        "tests/fixtures/h2/battle-ai-movement-static-v1.json": (
            "sf2-battle-ai-movement-static-v1"
        ),
        "tests/fixtures/h2/battle-ai-remaining-static-v1.json": (
            "sf2-battle-ai-remaining-static-v1"
        ),
        "tests/fixtures/h3/battle-ai-action-choice-v1.json": (
            "sf2-battle-ai-action-choice-runtime-v1"
        ),
    },
    "docs/design/contracts/battle-action-construction.md": {
        "tests/fixtures/h2/battle-actions-static-v1.json": (
            "sf2-battle-actions-static-v1"
        ),
    },
    "docs/design/contracts/battle-cutscene-routing.md": {
        "tests/fixtures/h2/battle-cutscene-data-static-v1.json": (
            "sf2-battle-cutscene-data-static-v1"
        ),
        "tests/fixtures/h2/battle-cutscenes-static-v1.json": (
            "sf2-battle-cutscenes-static-v1"
        ),
        "tests/fixtures/h2/battle-routing-data-static-v1.json": (
            "sf2-battle-routing-data-static-v1"
        ),
    },
    "docs/design/contracts/battle-encounter-definition.md": {
        "tests/fixtures/h2/battle-spriteset-data-static-v1.json": (
            "sf2-battle-spriteset-data-static-v1"
        ),
        "tests/fixtures/h2/battle-global-data-static-v1.json": (
            "sf2-battle-global-data-static-v1"
        ),
        "tests/fixtures/h2/battle-routing-data-static-v1.json": (
            "sf2-battle-routing-data-static-v1"
        ),
    },
    "docs/design/contracts/battle-scene-presentation.md": {
        "tests/fixtures/h2/battle-scene-engine-static-v1.json": (
            "sf2-battle-scene-engine-static-v1"
        ),
        "tests/fixtures/h2/battle-scene-animations-static-v1.json": (
            "sf2-battle-scene-animations-static-v1"
        ),
        "tests/fixtures/h2/battle-background-decode-v1.json": (
            "sf2-battle-background-decode-v1"
        ),
        "tests/fixtures/h2/battle-sprite-decode-v1.json": (
            "sf2-battle-sprite-decode-v1"
        ),
        "tests/fixtures/h2/battle-sprite-animation-static-v1.json": (
            "sf2-battle-sprite-animation-static-v1"
        ),
        "tests/fixtures/h2/battle-weapon-ground-decode-v1.json": (
            "sf2-battle-weapon-ground-decode-v1"
        ),
        "tests/fixtures/h2/battle-effect-graphics-decode-v1.json": (
            "sf2-battle-effect-graphics-decode-v1"
        ),
    },
    "docs/design/contracts/battle-control-lifecycle.md": {
        "tests/fixtures/h2/battle-loop-static-v1.json": "sf2-battle-loop-static-v1",
        "tests/fixtures/h2/battle-control-static-v1.json": (
            "sf2-battle-control-static-v1"
        ),
        "tests/fixtures/h3/battle01-turn-order-v1.json": (
            "sf2-battle01-turn-order-v1"
        ),
        "tests/fixtures/h3/turn-order-boundaries-v1.json": (
            "sf2-turn-order-boundaries-v1"
        ),
        "tests/fixtures/h3/battle01-region-activation-v1.json": (
            "sf2-battle01-region-activation-v1"
        ),
        "tests/fixtures/h3/battle01-secondary-activation-v1.json": (
            "sf2-battle01-secondary-activation-v1"
        ),
        "tests/fixtures/h3/after-turn-status-lifecycle-v1.json": (
            "sf2-after-turn-status-lifecycle-v1"
        ),
    },
    "docs/design/contracts/dialogue-system.md": {
        "tests/fixtures/h2/map-script-engine-static-v1.json": (
            "sf2-map-script-engine-static-v1"
        ),
        "tests/fixtures/h3/map-script-dialogue-v1.json": (
            "sf2-map-script-dialogue-runtime-v1"
        ),
    },
    "docs/design/contracts/debug-control-flow.md": {
        "tests/fixtures/h2/remaining-core-static-v1.json": (
            "sf2-remaining-core-static-v1"
        ),
    },
    "docs/design/contracts/sprite-dialogue-property-data.md": {
        "tests/fixtures/h2/sprite-dialogue-static-v1.json": (
            "sf2-sprite-dialogue-static-v1"
        ),
    },
    "docs/design/contracts/enemy-definition-data.md": {
        "tests/fixtures/h2/core-stats-data-static-v1.json": (
            "sf2-core-stats-data-static-v1"
        ),
        "tests/fixtures/h2/enemy-map-sprites-static-v1.json": (
            "sf2-enemy-map-sprites-static-v1"
        ),
    },
    "docs/design/contracts/global-flag-state.md": {
        "tests/fixtures/h2/common-stats-static-v1.json": (
            "sf2-common-stats-static-v1"
        ),
        "tests/fixtures/h2/tech-interrupts-static-v1.json": (
            "sf2-tech-interrupts-static-v1"
        ),
    },
    "docs/design/contracts/graphics-service-state.md": {
        "tests/fixtures/h2/tech-graphics-static-v1.json": (
            "sf2-tech-graphics-static-v1"
        ),
        "tests/fixtures/h2/special-sprite-decode-v1.json": (
            "sf2-special-sprite-decode-v1"
        ),
    },
    "docs/design/contracts/ui-graphics-asset-data.md": {
        "tests/fixtures/h2/ui-graphics-decode-v1.json": (
            "sf2-ui-graphics-decode-v1"
        ),
        "tests/fixtures/h2/icon-graphics-static-v1.json": (
            "sf2-icon-graphics-static-v1"
        ),
    },
    "docs/design/contracts/special-screen-asset-data.md": {
        "tests/fixtures/h2/special-screen-graphics-decode-v1.json": (
            "sf2-special-screen-graphics-decode-v1"
        ),
        "tests/fixtures/h2/special-screen-presentation-static-v1.json": (
            "sf2-special-screen-presentation-static-v1"
        ),
        "tests/fixtures/h2/witch-menu-graphics-static-v1.json": (
            "sf2-witch-menu-graphics-static-v1"
        ),
    },
    "docs/design/contracts/special-screen-control-flow.md": {
        "tests/fixtures/h2/special-screens-static-v1.json": (
            "sf2-special-screens-static-v1"
        ),
    },
    "docs/design/contracts/portrait-window-state.md": {
        "tests/fixtures/h2/common-menus-static-v1.json": (
            "sf2-common-menus-static-v1"
        ),
        "tests/fixtures/h2/portrait-graphics-decode-v1.json": (
            "sf2-portrait-graphics-decode-v1"
        ),
    },
    "docs/design/contracts/caravan-and-deals-state.md": {
        "tests/fixtures/h2/common-stats-static-v1.json": (
            "sf2-common-stats-static-v1"
        ),
    },
    "docs/design/contracts/new-game-state-initialization.md": {
        "tests/fixtures/h2/common-stats-static-v1.json": (
            "sf2-common-stats-static-v1"
        ),
    },
    "docs/design/contracts/party-membership-state.md": {
        "tests/fixtures/h2/common-stats-static-v1.json": (
            "sf2-common-stats-static-v1"
        ),
    },
    "docs/design/contracts/item-definition-data.md": {
        "tests/fixtures/h2/core-stats-data-static-v1.json": (
            "sf2-core-stats-data-static-v1"
        ),
        "tests/fixtures/h2/item-auxiliary-static-v1.json": (
            "sf2-item-auxiliary-static-v1"
        ),
    },
    "docs/design/contracts/spell-definition-data.md": {
        "tests/fixtures/h2/core-stats-data-static-v1.json": (
            "sf2-core-stats-data-static-v1"
        ),
    },
    "docs/design/contracts/spellbook-state.md": {
        "tests/fixtures/h2/common-stats-static-v1.json": (
            "sf2-common-stats-static-v1"
        ),
    },
    "docs/design/contracts/party-roster-state.md": {
        "tests/fixtures/h2/map-script-engine-static-v1.json": (
            "sf2-map-script-engine-static-v1"
        ),
        "tests/fixtures/h3/force-state-active-party-v1.json": (
            "sf2-force-state-active-party-runtime-v1"
        ),
        "tests/fixtures/h3/force-state-roster-death-v1.json": (
            "sf2-force-state-roster-death-runtime-v1"
        ),
    },
    "docs/design/contracts/exploration-control-flow.md": {
        "tests/fixtures/h2/gameflow-core-static-v1.json": (
            "sf2-gameflow-core-static-v1"
        ),
    },
    "docs/design/contracts/startup-control-flow.md": {
        "tests/fixtures/h2/gameflow-core-static-v1.json": (
            "sf2-gameflow-core-static-v1"
        ),
    },
    "docs/design/contracts/map-area-description-routing.md": {
        "tests/fixtures/h2/map-descriptions-static-v1.json": (
            "sf2-map-descriptions-static-v1"
        ),
    },
    "docs/design/contracts/map-entity-data.md": {
        "tests/fixtures/h2/map-entities-static-v1.json": (
            "sf2-map-entities-static-v1"
        ),
    },
    "docs/design/contracts/map-entry-routing-state.md": {
        "tests/fixtures/h2/common-maps-static-v1.json": (
            "sf2-common-maps-static-v1"
        ),
    },
    "docs/design/contracts/map-exploration.md": {
        "tests/fixtures/h2/map-content-static-v1.json": "sf2-map-content-static-v1",
        "tests/fixtures/h2/map-layout-decode-v1.json": "sf2-map-layout-decode-v1",
        "tests/fixtures/h2/canonical-map-import-v1.json": "sf2-canonical-map-import-v1",
        "tests/fixtures/h2/map-events-static-v1.json": "sf2-map-events-static-v1",
        "tests/fixtures/h2/map-init-static-v1.json": "sf2-map-init-static-v1",
        "tests/fixtures/h3/map-setup-selection-v1.json": (
            "sf2-map-setup-selection-runtime-v1"
        ),
        "tests/fixtures/h3/map-init-dispatch-v1.json": (
            "sf2-map-init-dispatch-runtime-v1"
        ),
        "tests/fixtures/h3/map-lifecycle-v1.json": "sf2-map-lifecycle-runtime-v1",
        "tests/fixtures/h3/map-script-control-audio-v1.json": (
            "sf2-map-script-control-audio-runtime-v1"
        ),
        "tests/fixtures/h3/map-script-transition-v1.json": (
            "sf2-map-script-transition-runtime-v1"
        ),
        "tests/fixtures/h3/map-block-mutation-v1.json": (
            "sf2-map-block-mutation-runtime-v1"
        ),
        "tests/fixtures/h3/map-block-copy-lifecycle-v1.json": (
            "sf2-map-block-copy-lifecycle-runtime-v1"
        ),
        "tests/fixtures/h3/entity-population-reload-v1.json": (
            "sf2-entity-population-reload-runtime-v1"
        ),
        "tests/fixtures/h3/map-interaction-trigger-v1.json": (
            "sf2-map-interaction-trigger-runtime-v1"
        ),
        "tests/fixtures/h3/map-camera-control-v1.json": (
            "sf2-map-camera-control-runtime-v1"
        ),
        "tests/fixtures/h3/map-script-entity-placement-v1.json": (
            "sf2-map-script-entity-placement-runtime-v1"
        ),
        "tests/fixtures/h3/map-script-entity-clone-v1.json": (
            "sf2-map-script-entity-clone-runtime-v1"
        ),
        "tests/fixtures/h3/map-script-ui-primary-v1.json": (
            "sf2-map-script-ui-primary-runtime-v1"
        ),
        "tests/fixtures/h3/map-script-screen-presentation-v1.json": (
            "sf2-map-script-screen-presentation-runtime-v1"
        ),
    },
    "docs/design/contracts/map-layout-data.md": {
        "tests/fixtures/h2/map-layout-decode-v1.json": (
            "sf2-map-layout-decode-v1"
        ),
    },
    "docs/design/contracts/map-palette-data.md": {
        "tests/fixtures/h2/map-palette-static-v1.json": (
            "sf2-map-palette-static-v1"
        ),
    },
    "docs/design/contracts/map-sprite-graphics-data.md": {
        "tests/fixtures/h2/map-sprite-decode-v1.json": (
            "sf2-map-sprite-decode-v1"
        ),
    },
    "docs/design/contracts/map-tileset-data.md": {
        "tests/fixtures/h2/map-tileset-decode-v1.json": (
            "sf2-map-tileset-decode-v1"
        ),
    },
    "docs/design/contracts/battlefield-navigation.md": {
        "tests/fixtures/h2/battlefield-static-v1.json": "sf2-battlefield-static-v1",
        "tests/fixtures/h2/battle-terrain-decode-v1.json": (
            "sf2-battle-terrain-decode-v1"
        ),
        "tests/fixtures/h3/battlefield-movement-matrix-v1.json": (
            "sf2-battlefield-movement-runtime-v1"
        ),
    },
    "docs/design/contracts/combatant-state-access.md": {
        "tests/fixtures/h2/common-stats-static-v1.json": (
            "sf2-common-stats-static-v1"
        ),
        "tests/fixtures/h3/stat-clamp-boundaries-v1.json": (
            "sf2-stat-clamp-boundaries-v1"
        ),
    },
    "docs/design/contracts/combat-resolution.md": {
        "tests/fixtures/h3/muddle-action-guard-v1.json": (
            "sf2-muddle-action-guard-both-sides-v1"
        ),
        "tests/fixtures/h3/muddle-confusion-v1.json": (
            "sf2-muddle-confusion-truth-table-v1"
        ),
        "tests/fixtures/h3/physical-damage-v1.json": "sf2-physical-damage-land-archer-v1",
        "tests/fixtures/h3/physical-damage-application-v1.json": (
            "sf2-physical-damage-application-v1"
        ),
        "tests/fixtures/h3/battle-scene-replay-v1.json": "sf2-battle-scene-replay-v1",
        "tests/fixtures/h3/battle-exp-level-up-v1.json": "sf2-battle-exp-level-up-v1",
        "tests/fixtures/h3/kill-exp-level-difference-v1.json": (
            "sf2-kill-exp-level-difference-v1"
        ),
        "tests/fixtures/h3/award-exp-randomization-v1.json": (
            "sf2-award-exp-randomization-v1"
        ),
        "tests/fixtures/h3/exp-command-boundaries-v1.json": (
            "sf2-exp-command-boundaries-v1"
        ),
        "tests/fixtures/h2/enemy-gold-v1.json": "sf2-enemy-gold-v1",
        "tests/fixtures/h2/enemy-item-drops-v1.json": "sf2-enemy-item-drops-v1",
        "tests/fixtures/h3/gold-boundaries-v1.json": "sf2-gold-boundaries-v1",
        "tests/fixtures/h3/enemy-item-drop-behavior-v1.json": (
            "sf2-enemy-item-drop-behavior-v1"
        ),
        "tests/fixtures/h3/attack-chain-v1.json": "sf2-attack-chain-double-counter-v1",
        "tests/fixtures/h3/dodge-v1.json": "sf2-successful-airborne-dodge-v1",
        "tests/fixtures/h3/lethal-followup-v1.json": "sf2-lethal-followup-validation-v1",
        "tests/fixtures/h3/counter-range-v1.json": "sf2-counter-range-validation-v1",
        "tests/fixtures/h3/counter-sleep-v1.json": "sf2-counter-sleep-validation-v1",
        "tests/fixtures/h3/counter-stun-v1.json": "sf2-counter-stun-validation-v1",
        "tests/fixtures/h3/counter-same-side-v1.json": "sf2-counter-same-side-validation-v1",
        "tests/fixtures/h3/counter-burst-rock-v1.json": (
            "sf2-counter-burst-rock-validation-v1"
        ),
        "tests/fixtures/h3/counter-special-enemies-v1.json": (
            "sf2-counter-special-enemies-validation-v1"
        ),
        "tests/fixtures/h3/double-validation-v1.json": "sf2-double-validation-gates-v1",
    },
    "docs/design/contracts/level-up.md": {
        "tests/fixtures/h3/stat-gain-v1.json": "sf2-calculate-stat-gain-startup-v1",
        "tests/fixtures/h3/level-up-v1.json": "sf2-level-up-tort-boundary-v1",
        "tests/fixtures/h3/level-up-boundaries-v1.json": "sf2-level-up-boundaries-v1",
        "tests/fixtures/h3/level-up-refresh-v1.json": "sf2-level-up-refresh-v1",
        "tests/fixtures/h3/ally-initialization-prowess-v1.json": "sf2-karna-heal3-prowess-v1",
        "tests/fixtures/h3/stat-clamp-boundaries-v1.json": "sf2-stat-clamp-boundaries-v1",
        "tests/fixtures/h3/enemy-curse-suppression-v1.json": (
            "sf2-enemy-curse-suppression-v1"
        ),
        "tests/fixtures/h3/battle-exp-level-up-v1.json": "sf2-battle-exp-level-up-v1",
        "tests/fixtures/h3/exp-command-boundaries-v1.json": (
            "sf2-exp-command-boundaries-v1"
        ),
    },
    "docs/design/contracts/spell-resolution.md": {
        "tests/fixtures/h3/spell-damage-resistance-v1.json": (
            "sf2-spell-damage-resistance-v1"
        ),
        "tests/fixtures/h3/spell-damage-exp-v1.json": "sf2-spell-damage-exp-v1",
        "tests/fixtures/h3/spell-summon-division-v1.json": (
            "sf2-spell-summon-division-v1"
        ),
        "tests/fixtures/h3/spell-healing-v1.json": "sf2-heal1-self-recovery-v1",
        "tests/fixtures/h3/spell-healing-exp-boundaries-v1.json": (
            "sf2-healing-exp-boundaries-v1"
        ),
        "tests/fixtures/h3/spell-aura-targets-v1.json": (
            "sf2-aura-target-geometry-v1"
        ),
        "tests/fixtures/h3/spell-detox-v1.json": (
            "sf2-detox-level-status-matrix-v1"
        ),
        "tests/fixtures/h3/spell-attack-v1.json": (
            "sf2-attack1-fresh-and-recast-v1"
        ),
        "tests/fixtures/h3/spell-muddle-v1.json": (
            "sf2-muddle2-resistance-matrix-v1"
        ),
        "tests/fixtures/h3/spell-muddle1-v1.json": (
            "sf2-muddle1-fresh-recast-guard-v1"
        ),
        "tests/fixtures/h3/spell-status-sleep-v1.json": (
            "sf2-sleep-resistance-matrix-v1"
        ),
        "tests/fixtures/h3/spell-desoul-v1.json": "sf2-desoul-instant-death-v1",
        "tests/fixtures/h3/spell-mp-absorb-v1.json": "sf2-spoit-mp-absorb-v1",
        "tests/fixtures/h3/spell-boost-v1.json": "sf2-boost1-fresh-and-recast-v1",
        "tests/fixtures/h3/spell-slow-v1.json": "sf2-slow1-status-resistance-v1",
        "tests/fixtures/h3/spell-dispel-v1.json": (
            "sf2-dispel1-spell-gate-and-recast-v1"
        ),
        "tests/fixtures/h3/spell-silence-gate-v1.json": (
            "sf2-silenced-caster-blocks-blaze1-v1"
        ),
        "tests/fixtures/h3/after-turn-status-lifecycle-v1.json": (
            "sf2-after-turn-status-lifecycle-v1"
        ),
    },
    "docs/design/contracts/service-interactions.md": {
        "tests/fixtures/h2/common-menus-static-v1.json": "sf2-common-menus-static-v1",
        "tests/fixtures/h3/church-raise-lifecycle-v1.json": (
            "sf2-church-raise-lifecycle-runtime-v1"
        ),
        "tests/fixtures/h3/church-cure-lifecycle-v1.json": (
            "sf2-church-cure-lifecycle-runtime-v1"
        ),
    },
    "docs/design/contracts/save-system.md": {
        "tests/fixtures/h2/tech-services-static-v1.json": "sf2-tech-services-static-v1",
        "tests/fixtures/h3/witch-save-actions-v1.json": "sf2-witch-save-actions-runtime-v1",
        "tests/fixtures/h3/witch-save-menu-actions-v1.json": (
            "sf2-witch-save-menu-actions-runtime-v1"
        ),
        "tests/fixtures/h3/witch-new-game-lifecycle-v1.json": (
            "sf2-witch-new-game-lifecycle-runtime-v1"
        ),
    },
    "docs/design/contracts/input-system.md": {
        "tests/fixtures/h2/tech-services-static-v1.json": "sf2-tech-services-static-v1",
        "tests/fixtures/h2/tech-interrupts-static-v1.json": "sf2-tech-interrupts-static-v1",
    },
    "docs/design/contracts/randomness.md": {
        "tests/fixtures/h2/tech-services-static-v1.json": "sf2-tech-services-static-v1",
        "tests/fixtures/h3/rng-v1.json": "sf2-rng-generate-random-number-v1",
        "tests/fixtures/h3/debug-rng-v1.json": "sf2-rng-debug-override-v1",
        "tests/fixtures/h3/random-services-v1.json": (
            "sf2-random-services-matrix-runtime-v1"
        ),
        "tests/fixtures/h3/battle-ai-action-choice-v1.json": (
            "sf2-battle-ai-action-choice-runtime-v1"
        ),
    },
    "docs/design/contracts/rom-header-data.md": {
        "tests/fixtures/h2/remaining-core-static-v1.json": (
            "sf2-remaining-core-static-v1"
        ),
    },
    "docs/design/contracts/text-and-font-system.md": {
        "tests/fixtures/h2/text-huffman-static-v1.json": (
            "sf2-text-huffman-static-v1"
        ),
        "tests/fixtures/h2/text-banks-static-v1.json": (
            "sf2-text-banks-static-v1"
        ),
        "tests/fixtures/h2/variable-width-font-static-v1.json": (
            "sf2-variable-width-font-static-v1"
        ),
    },
    "docs/design/contracts/ui-layout-data.md": {
        "tests/fixtures/h2/ui-layout-static-v1.json": "sf2-ui-layout-static-v1",
    },
    "docs/design/contracts/window-system.md": {
        "tests/fixtures/h2/remaining-core-static-v1.json": "sf2-remaining-core-static-v1",
        "tests/fixtures/h2/ui-layout-static-v1.json": "sf2-ui-layout-static-v1",
        "tests/fixtures/h2/tech-interrupts-static-v1.json": "sf2-tech-interrupts-static-v1",
    },
    "docs/design/contracts/interrupt-dma-and-trap-state.md": {
        "tests/fixtures/h2/tech-interrupts-static-v1.json": "sf2-tech-interrupts-static-v1",
    },
}


def verify_design_contracts() -> dict[str, Any]:
    docs_index = repo_path("docs/README.md").read_text(encoding="utf-8")
    fixture_count = 0
    for document_relative, references in CONTRACTS.items():
        document_path = repo_path(document_relative)
        if not document_path.is_file():
            raise ValueError(f"missing design contract: {document_relative}")
        document = document_path.read_text(encoding="utf-8")
        index_reference = f"./{document_path.relative_to(repo_path('docs')).as_posix()}"
        if index_reference not in docs_index:
            raise ValueError(f"docs/README.md does not index {document_relative}")
        if "**Confirmed" not in document or "**Unknown" not in document:
            raise ValueError(f"design contract lacks evidence labels: {document_relative}")
        for fixture_relative, fixture_id in references.items():
            fixture_path = repo_path(fixture_relative)
            if not fixture_path.is_file():
                raise ValueError(f"missing referenced fixture: {fixture_relative}")
            fixture = load_json(fixture_path)
            if fixture["id"] != fixture_id:
                raise ValueError(
                    f"fixture ID mismatch at {fixture_relative}: expected {fixture_id}, "
                    f"got {fixture['id']}"
                )
            if fixture_relative not in document or fixture_id not in document:
                raise ValueError(
                    f"design contract does not trace {fixture_id} to {fixture_relative}"
                )
            fixture_count += 1
    return {
        "Documents": len(CONTRACTS),
        "FixtureReferences": fixture_count,
        "EvidenceLabels": "Confirmed,Unknown",
        "Status": "PASS",
    }
