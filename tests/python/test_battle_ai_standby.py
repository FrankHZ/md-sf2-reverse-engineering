from collections import Counter
from itertools import product

from sf2tool.h2.battle_ai import _standby_eligibility_outcome


def test_standby_eligibility_matrix_has_one_move_order_configuration() -> None:
    outcomes = {
        state: _standby_eligibility_outcome(*state)
        for state in product((False, True), repeat=4)
    }

    assert Counter(outcomes.values()) == {
        "stay": 11,
        "regular-move": 4,
        "move-order": 1,
    }
    assert outcomes[(True, False, False, True)] == "move-order"


def test_configured_order_and_matching_trigger_force_stay() -> None:
    assert _standby_eligibility_outcome(True, False, True, False) == "stay"
    assert _standby_eligibility_outcome(False, True, False, True) == "stay"
