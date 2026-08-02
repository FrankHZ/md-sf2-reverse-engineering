from __future__ import annotations

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OBSERVER_PATH = ROOT / "scripts" / "Observe-H3Battle01TurnOrder.ps1"
TURN_ORDER_FIXTURE_PATH = ROOT / "tests" / "fixtures" / "h3" / "battle01-turn-order-v1.json"
BATTLE_TEST_FIXTURE_PATH = ROOT / "tests" / "fixtures" / "h2" / "remaining-core-static-v1.json"
CUTSCENE_FIXTURE_PATH = ROOT / "tests" / "fixtures" / "h2" / "battle-cutscenes-static-v1.json"


def _load_json(path: Path) -> dict[str, object]:
    return json.loads(path.read_text(encoding="utf-8"))


def test_battle01_harness_uses_nonzero_cutscene_prompt_and_owner_facts() -> None:
    observer = OBSERVER_PATH.read_text(encoding="utf-8")
    prompt_one = 'if prompt_count == 1 then pulse("Right"); pulse("C")'
    prompt_two = 'elseif prompt_count == 2 then pulse("Right"); pulse("C") end'

    assert prompt_one in observer
    assert prompt_two in observer
    assert observer.index(prompt_one) < observer.index(prompt_two)
    assert 'elseif prompt_count == 2 then pulse("C") end' not in observer

    turn_order_fixture = _load_json(TURN_ORDER_FIXTURE_PATH)
    assert turn_order_fixture["inputHarness"] == (
        "original debug-mode sequence and Battle Test UI; prompt 2 selects nonzero option 1 with "
        "Right+C, setting the shared intro-cutscene flag; Player 2 Start remains a fallback during "
        "later battle-scene playback"
    )

    battle_test_fixture = _load_json(BATTLE_TEST_FIXTURE_PATH)
    flow = battle_test_fixture["expected"]["debugFacts"]["battleTest"]["flow"]
    assert flow["cutscenePromptRange"] == [0, 1]
    assert flow["nonzeroCutsceneSetsFlagOffsetLabel"] == "BATTLE_INTRO_CUTSCENE_FLAGS_START"

    cutscene_fixture = _load_json(CUTSCENE_FIXTURE_PATH)
    assert cutscene_fixture["expected"]["cutsceneFacts"]["intro"] == {
        "beforeBattleChecksSharedIntroFlag": True,
        "beforeBattleSetsFlag": False,
        "battleStartChecksSharedIntroFlag": True,
        "battleStartSetsFlagBeforeScript": True,
        "dispatchesByCurrentBattle": True,
    }
