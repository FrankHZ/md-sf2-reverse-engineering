"""Strict closure tests for central research-index later-owner normalization."""

from __future__ import annotations

import importlib
import subprocess
import sys
from copy import deepcopy
from dataclasses import FrozenInstanceError, replace
from pathlib import Path
from typing import Any

import pytest

import sf2tool.research_index as research_index
from sf2tool.jsonio import load_json

ROOT = Path(__file__).resolve().parents[2]
INDEX = ROOT / "manifests/research-index.json"

_OWNER_IDS = (
    "sf2-map-event-flag-route-selection-static-v1",
    "sf2-map-event-cross-program-flag-state-static-v1",
    "sf2-map-event-flag-lifecycle-state-static-v1",
    "sf2-map-event-scripted-transition-state-static-v1",
    "sf2-map-event-tactical-base-quote-state-static-v1",
    "sf2-map-event-random-battle-state-static-v1",
    "sf2-map-event-combatant-state-static-v1",
    "sf2-map-event-item-transactions-static-v1",
    "sf2-map-event-interaction-state-static-v1",
)

_MIGRATED_CONSUMER_TESTS = (
    "test_common_stats.py",
    "test_field_item_effects.py",
    "test_field_menu_control.py",
    "test_field_search_control.py",
    "test_map3_battle01_action_completion.py",
    "test_map3_battle01_victory_return.py",
    "test_map_event_combatant_state.py",
    "test_map_event_cross_program_flag_state.py",
    "test_map_event_dialogue_state.py",
    "test_map_event_direct_control.py",
    "test_map_event_direct_handoff.py",
    "test_map_event_direct_state.py",
    "test_map_event_flag_lifecycle_state.py",
    "test_map_event_flag_route_selection.py",
    "test_map_event_interaction_state.py",
    "test_map_event_item_transactions.py",
    "test_map_event_predicate_results.py",
    "test_map_event_random_battle_state.py",
    "test_map_event_request_consumption.py",
    "test_map_event_request_state.py",
    "test_map_event_scripted_transition_state.py",
    "test_map_event_tactical_base_quote_state.py",
)

_COMPATIBILITY_WRAPPERS = (
    (
        "sf2tool.h2.map_event_flag_route_selection",
        "normalize_map_event_flag_route_selection_later_owner_index",
    ),
    (
        "sf2tool.h2.map_event_cross_program_flag_state",
        "normalize_map_event_cross_program_flag_state_later_owner_index",
    ),
    (
        "sf2tool.h2.map_event_flag_lifecycle_state",
        "normalize_map_event_flag_lifecycle_state_later_owner_index",
    ),
    (
        "sf2tool.h2.map_event_scripted_transition_state",
        "normalize_map_event_scripted_transition_state_later_owner_index",
    ),
    (
        "sf2tool.h2.map_event_tactical_base_quote_state",
        "normalize_map_event_tactical_base_quote_state_later_owner_index",
    ),
    (
        "sf2tool.h2.map_event_random_battle_state",
        "normalize_map_event_random_battle_state_later_owner_index",
    ),
    (
        "sf2tool.h2.map_event_combatant_state",
        "normalize_map_event_combatant_state_later_owner_index",
    ),
    (
        "sf2tool.h2.map_event_item_transactions",
        "normalize_map_event_item_transactions_later_owner_index",
    ),
    (
        "sf2tool.h2.map_event_interaction_state",
        "normalize_interaction_state_later_owner_index",
    ),
)


def test_registry_is_frozen_and_each_transition_is_exact_and_deep() -> None:
    steps = research_index._LATER_OWNER_STEPS
    assert isinstance(steps, tuple)
    assert tuple(step.owner_id for step in steps) == _OWNER_IDS
    assert research_index._validate_later_owner_steps(steps) == {
        step.owner_id: step for step in steps
    }
    with pytest.raises(FrozenInstanceError):
        steps[0].owner_id = "changed"  # type: ignore[misc]

    current = load_json(INDEX)
    untouched = deepcopy(current)
    assert research_index._canonical_index_sha256(current) == steps[0].state_sha256
    for step in steps:
        owner_state = research_index._normalize_current_index_to_owner_state(
            current, owner_id=step.owner_id
        )
        predecessor = research_index.normalize_current_index_to_owner_predecessor(
            current, owner_id=step.owner_id
        )
        direct = research_index._resolve_later_owner_remover(step)(owner_state)
        assert research_index._canonical_index_sha256(owner_state) == step.state_sha256
        assert research_index._canonical_index_sha256(predecessor) == step.predecessor_sha256
        assert direct == predecessor
        assert direct is not owner_state
    assert current == untouched


@pytest.mark.parametrize(
    ("steps", "error"),
    (
        ((), "registry shape drift"),
        (
            (
                research_index._LATER_OWNER_STEPS[0],
                replace(
                    research_index._LATER_OWNER_STEPS[1],
                    owner_id=research_index._LATER_OWNER_STEPS[0].owner_id,
                ),
                *research_index._LATER_OWNER_STEPS[2:],
            ),
            "duplicate research-index later-owner ID",
        ),
        (
            (
                research_index._LATER_OWNER_STEPS[0],
                replace(
                    research_index._LATER_OWNER_STEPS[1],
                    remover=research_index._LATER_OWNER_STEPS[0].remover,
                ),
                *research_index._LATER_OWNER_STEPS[2:],
            ),
            "duplicate research-index later-owner remover",
        ),
        (
            (
                replace(
                    research_index._LATER_OWNER_STEPS[0],
                    predecessor_owner_id="missing-owner",
                ),
                *research_index._LATER_OWNER_STEPS[1:],
            ),
            "missing research-index later-owner predecessor",
        ),
        (
            tuple(reversed(research_index._LATER_OWNER_STEPS)),
            "registry order drift",
        ),
        (
            (
                replace(
                    research_index._LATER_OWNER_STEPS[0],
                    predecessor_sha256="0" * 64,
                ),
                *research_index._LATER_OWNER_STEPS[1:],
            ),
            "registry continuity drift",
        ),
        (
            (
                replace(research_index._LATER_OWNER_STEPS[0], remover="invalid"),
                *research_index._LATER_OWNER_STEPS[1:],
            ),
            "registry entry drift",
        ),
    ),
)
def test_registry_rejects_missing_duplicate_unknown_and_disordered_graphs(
    steps: tuple[research_index.LaterOwnerStep, ...], error: str
) -> None:
    with pytest.raises(ValueError, match=error):
        research_index._validate_later_owner_steps(steps)


def test_unknown_target_and_noncanonical_head_fail_before_any_remover() -> None:
    current = load_json(INDEX)
    with pytest.raises(ValueError, match="unknown research-index later owner"):
        research_index.normalize_current_index_to_owner_predecessor(
            current, owner_id="unknown-owner"
        )
    altered = deepcopy(current)
    altered["records"][0]["documents"].append("docs/research/unexpected.md")
    with pytest.raises(ValueError, match="head state drift"):
        research_index.normalize_current_index_to_owner_predecessor(
            altered, owner_id=_OWNER_IDS[-1]
        )


def test_lazy_resolver_rejects_identity_callable_and_signature_drift(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    step = research_index._LATER_OWNER_STEPS[0]
    with pytest.raises(ValueError, match="module identity drift"):
        research_index._resolve_later_owner_remover(
            replace(step, owner_id="wrong-owner")
        )
    with pytest.raises(ValueError, match="remover is unavailable"):
        research_index._resolve_later_owner_remover(
            replace(step, remover=step.remover.rsplit(":", 1)[0] + ":missing")
        )
    module = importlib.import_module(step.remover.partition(":")[0])
    monkeypatch.setattr(
        module, "_invalid_test_remover", lambda _index, _extra: {}, raising=False
    )
    with pytest.raises(ValueError, match="remover signature drift"):
        research_index._resolve_later_owner_remover(
            replace(step, remover=f"{module.__name__}:_invalid_test_remover")
        )


def test_compatibility_wrappers_delegate_once_without_recursion(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    sentinel_input = {"sentinel": "input"}
    sentinel_output = {"sentinel": "output"}
    calls: list[tuple[dict[str, Any], str]] = []

    def fake(index: dict[str, Any], *, owner_id: str) -> dict[str, Any]:
        calls.append((index, owner_id))
        return sentinel_output

    monkeypatch.setattr(research_index, "normalize_current_index_to_owner_predecessor", fake)
    for (module_name, wrapper_name), owner_id in zip(
        _COMPATIBILITY_WRAPPERS, _OWNER_IDS, strict=True
    ):
        wrapper = getattr(importlib.import_module(module_name), wrapper_name)
        assert wrapper(sentinel_input) is sentinel_output
        assert calls[-1] == (sentinel_input, owner_id)
    assert len(calls) == len(_OWNER_IDS)


def test_all_historical_consumer_tests_use_the_central_api_without_owner_chains() -> None:
    assert len(_MIGRATED_CONSUMER_TESTS) == 22
    forbidden = (
        "normalize_map_event_",
        "normalize_interaction_state_later_owner_index",
        "_remove_cross_program_flag_lifecycle_deltas",
    )
    for name in _MIGRATED_CONSUMER_TESTS:
        source = (ROOT / "tests/python" / name).read_text(encoding="utf-8")
        assert "from sf2tool.research_index import" in source, name
        assert "normalize_current_index_to_owner_predecessor" in source, name
        assert not any(token in source for token in forbidden), name


def test_one_synthetic_future_step_preserves_every_historical_target() -> None:
    current = load_json(INDEX)
    synthetic = deepcopy(current)
    future_owner = "synthetic-future-owner-static-v1"
    marker = {"ownerId": future_owner, "version": 1}
    synthetic["syntheticLaterOwner"] = marker

    def remove_synthetic(index: dict[str, Any]) -> dict[str, Any]:
        if set(index) != {*current, "syntheticLaterOwner"}:
            raise ValueError("synthetic future owner root drift")
        if index["syntheticLaterOwner"] != marker:
            raise ValueError("synthetic future owner marker drift")
        predecessor = deepcopy(index)
        del predecessor["syntheticLaterOwner"]
        if predecessor != current:
            raise ValueError("synthetic future owner unrelated drift")
        return predecessor

    future_step = research_index.LaterOwnerStep(
        owner_id=future_owner,
        predecessor_owner_id=_OWNER_IDS[0],
        remover="synthetic.future:remove_synthetic",
        state_sha256=research_index._canonical_index_sha256(synthetic),
        predecessor_sha256=research_index._canonical_index_sha256(current),
    )
    steps = (future_step, *research_index._LATER_OWNER_STEPS)
    calls: list[str] = []

    def resolver(step: research_index.LaterOwnerStep) -> research_index._LaterOwnerRemover:
        calls.append(step.owner_id)
        if step.owner_id == future_owner:
            return remove_synthetic
        return research_index._resolve_later_owner_remover(step)

    assert research_index._validate_later_owner_steps(steps)
    for owner_id in _OWNER_IDS:
        expected = research_index.normalize_current_index_to_owner_predecessor(
            current, owner_id=owner_id
        )
        actual = research_index._normalize_current_index(
            synthetic,
            owner_id=owner_id,
            include_owner=True,
            steps=steps,
            resolver=resolver,
        )
        assert actual == expected
    assert calls.count(future_owner) == len(_OWNER_IDS)


@pytest.mark.parametrize(
    "imports",
    (
        (
            "sf2tool.research_index",
            "sf2tool.h2.map_event_flag_route_selection",
            "sf2tool.h2.map_event_interaction_state",
        ),
        (
            "sf2tool.h2.map_event_interaction_state",
            "sf2tool.h2.map_event_flag_route_selection",
            "sf2tool.research_index",
        ),
    ),
)
def test_research_index_and_h2_import_orders_are_cycle_free(imports: tuple[str, ...]) -> None:
    code = ";".join(f"import {module}" for module in imports)
    result = subprocess.run(
        [sys.executable, "-c", code],
        cwd=ROOT,
        check=False,
        capture_output=True,
        text=True,
    )
    assert result.returncode == 0, result.stderr
