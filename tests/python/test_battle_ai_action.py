from sf2tool.h3.battle_ai_action import FIXTURE, _model_case, _thinking_rng_step
from sf2tool.jsonio import load_json


def test_thinking_rng_accepts_both_range_two_results() -> None:
    assert _thinking_rng_step(51, 2) == (0, 0)
    assert _thinking_rng_step(104, 2) == (1, 1)


def test_action_choice_model_covers_every_viability_mask() -> None:
    fixture = load_json(FIXTURE)
    modeled = {case["id"]: _model_case(case) for case in fixture["cases"]}

    assert [
        modeled[name]["action"]
        for name in (
            "none",
            "physical-only",
            "spell-only",
            "item-only",
            "spell-item-roll0",
            "physical-spell-roll0",
            "physical-item-roll0",
            "all-roll0",
        )
    ] == [3, 0, 1, 2, 1, 0, 0, 1]
    assert modeled["all-roll1"]["action"] == 2
    assert modeled["all-roll1"]["finalSeed"] == modeled["all-roll1"]["seed"]
    assert modeled["all-roll1"]["finalThinkingSeed"] == 1
