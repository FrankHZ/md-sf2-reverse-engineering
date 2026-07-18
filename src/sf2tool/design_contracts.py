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
