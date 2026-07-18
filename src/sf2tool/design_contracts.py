from __future__ import annotations

from typing import Any

from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path

CONTRACTS = {
    "docs/design/combat-resolution.md": {
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
    "docs/design/level-up.md": {
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
    "docs/design/spell-resolution.md": {
        "tests/fixtures/h3/spell-damage-resistance-v1.json": (
            "sf2-spell-damage-resistance-v1"
        ),
        "tests/fixtures/h3/spell-summon-division-v1.json": (
            "sf2-spell-summon-division-v1"
        ),
        "tests/fixtures/h3/spell-healing-v1.json": "sf2-heal1-self-recovery-v1",
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
}


def verify_design_contracts() -> dict[str, Any]:
    docs_index = repo_path("docs/README.md").read_text(encoding="utf-8")
    fixture_count = 0
    for document_relative, references in CONTRACTS.items():
        document_path = repo_path(document_relative)
        if not document_path.is_file():
            raise ValueError(f"missing design contract: {document_relative}")
        document = document_path.read_text(encoding="utf-8")
        index_reference = f"./design/{document_path.name}"
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
