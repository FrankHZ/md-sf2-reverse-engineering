from sf2tool.h3.entity_movement import (
    FIXTURE,
    SCHEMA,
    model_entity_movement_case,
)
from sf2tool.jsonio import load_json, validate_json


def test_entity_movement_fixture_matches_independent_model() -> None:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="entity movement test fixture")
    assert len(fixture["cases"]) == 13
    assert sum(case["ticks"] for case in fixture["cases"]) == 20
    for case in fixture["cases"]:
        assert case["expected"] == model_entity_movement_case(case)


def test_entity_movement_matrix_closes_queued_behavior_families() -> None:
    fixture = load_json(FIXTURE)
    ids = {case["id"] for case in fixture["cases"]}
    assert {
        "wait-threshold-three-ticks",
        "relative-unblocked-start-and-step",
        "relative-blocked-yields",
        "absolute-unblocked-start-and-step",
        "absolute-blocked-yields",
        "x-acceleration-three-ticks",
        "x-deceleration-two-ticks",
        "diagonal-facing-animation",
        "animation-disabled-minus-one",
        "stationary-animation-clamp",
        "arrival-layer-two",
        "arrival-layer-zero",
        "arrival-immersed-toggle",
    } == ids
