from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h2.map_event_combatant_state as combatant_state_module
from sf2tool.h2.map_event_combatant_state import (
    FIXTURE,
    SCHEMA,
    _remove_map_event_combatant_state_later_owner_index_delta,
    _validate_order,
    build_map_event_combatant_state_contract,
    normalize_map_event_combatant_state_later_owner_index,
)
from sf2tool.h2.map_event_item_transactions import (
    normalize_map_event_item_transactions_later_owner_index,
)
from sf2tool.h2.map_event_random_battle_state import (
    _remove_map_event_random_battle_state_later_owner_index_delta,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json

ROOT = Path(__file__).resolve().parents[2]
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"
INDEX = ROOT / "manifests/research-index.json"


def test_complete_static_contract_matches_closed_fixture() -> None:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map-event combatant state fixture")
    _validate_order(fixture)
    assert build_map_event_combatant_state_contract(ROM, UPSTREAM) == fixture
    assert fixture["summary"] == {
        "sourceIdentityCount": 6,
        "motherProgramContextCount": 914,
        "positiveProgramContextCount": 2,
        "zeroProgramContextCount": 912,
        "physicalProgramCount": 2,
        "contextOperationCount": 23,
        "physicalOperationCount": 23,
        "contextEncodedByteCount": 98,
        "physicalEncodedByteCount": 98,
        "physicalLabelCount": 3,
        "eventServiceMacroPhysicalOperationCount": 5,
        "rawInstructionPhysicalOperationCount": 4,
        "rawControlPhysicalOperationCount": 14,
        "statCallCount": 9,
        "allySelectorCount": 3,
        "restorationChainCount": 2,
        "resultPredicateCount": 1,
        "anchorCount": 35,
    }


def test_schema_rejects_public_boundary_and_order_drift() -> None:
    fixture = load_json(FIXTURE)
    broken = deepcopy(fixture)
    broken["sourceContext"]["privateRomBytes"] = "00"
    with pytest.raises(ValueError, match="schema validation"):
        validate_json(broken, SCHEMA, owner="broken")
    broken = deepcopy(fixture)
    broken["eventCombatantState"]["serviceCallOrder"][:2] = reversed(
        broken["eventCombatantState"]["serviceCallOrder"][:2]
    )
    with pytest.raises(ValueError, match="order drift"):
        _validate_order(broken)


@pytest.mark.parametrize(
    "mutate",
    [
        lambda value: value["eventCombatantState"]["sourceFiles"].update({"extra": {}}),
        lambda value: value["eventCombatantState"]["programContexts"]["map20-zone-event0"][
            "operations"
        ][1]["target"].update({"extra": 1}),
        lambda value: value["eventCombatantState"]["serviceEntries"]["GetMaxHp"].update(
            {"extra": 1}
        ),
        lambda value: value["eventCombatantState"]["serviceCalls"].update({"extra": {}}),
        lambda value: value["eventCombatantState"]["restorationChains"]["Sarah"].update(
            {"extra": 1}
        ),
        lambda value: value["eventCombatantState"]["resultPredicate"].update({"extra": 1}),
        lambda value: value["eventCombatantState"]["digests"].update({"extra": "0" * 64}),
    ],
)
def test_schema_recursively_rejects_nested_public_fields(mutate: object) -> None:
    broken = deepcopy(load_json(FIXTURE))
    mutate(broken)
    with pytest.raises(ValueError, match="schema validation"):
        validate_json(broken, SCHEMA, owner="nested public boundary")


def _program(value: dict[str, object], symbol: str) -> dict[str, object]:
    return next(
        program
        for family in ("entityTargetPrograms", "zoneTargetPrograms", "itemTargetPrograms")
        for program in value[family]
        if program["canonicalSymbol"] == symbol
    )


def test_retained_source_h1_rom_shape_mutations_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    original = load_map_events_fixture()["expected"]
    monkeypatch.setattr(
        combatant_state_module,
        "_retained_owners",
        lambda _map_events: load_json(FIXTURE)["retainedOwners"],
    )

    broken = deepcopy(original)
    broken["entityTargetPrograms"].pop()
    with pytest.raises(ValueError, match="mother corpus selection drift"):
        build_map_event_combatant_state_contract(ROM, UPSTREAM, map_events_override=broken)

    broken = deepcopy(original)
    _program(broken, "Map20_1F5_ZoneEvent0")["operations"][1]["sourceMnemonic"] = "jmp"
    with pytest.raises(ValueError, match="source operation drift"):
        build_map_event_combatant_state_contract(ROM, UPSTREAM, map_events_override=broken)

    broken = deepcopy(original)
    table = next(
        table
        for table in broken["categories"]["zoneEvents"]["tables"]
        if table["symbol"] == "ms_map20_flag501_ZoneEvents"
    )
    table["address"] += 2
    with pytest.raises(ValueError, match="source-table mutation"):
        build_map_event_combatant_state_contract(ROM, UPSTREAM, map_events_override=broken)

    broken = deepcopy(original)
    _program(broken, "Map20_1F5_ZoneEvent0")["operations"][1]["target"][
        "effectiveTargetAddress"
    ] += 2
    with pytest.raises(ValueError, match="alias/effective target drift"):
        build_map_event_combatant_state_contract(ROM, UPSTREAM, map_events_override=broken)

    broken = deepcopy(original)
    _program(broken, "Map67_ZoneEvent0")["operations"][6]["sizeSuffix"] = ".l"
    with pytest.raises(ValueError, match="CurrentHp predicate shape drift"):
        build_map_event_combatant_state_contract(ROM, UPSTREAM, map_events_override=broken)


def _record(index: dict[str, object], record_id: str) -> dict[str, object]:
    return next(record for record in index["records"] if record["id"] == record_id)


def test_strict_later_owner_normalizer_reconstructs_only_the_exact_delta() -> None:
    index = _remove_map_event_random_battle_state_later_owner_index_delta(load_json(INDEX))
    predecessor = _remove_map_event_combatant_state_later_owner_index_delta(index)
    assert len(index["records"]) - len(predecessor["records"]) == 1
    assert "stats.combatant-getters" not in {record["id"] for record in predecessor["records"]}
    assert (
        sum(len(record["addresses"]) for record in index["records"])
        - sum(len(record["addresses"]) for record in predecessor["records"])
        == 10
    )
    assert (
        sum(
            len(evidence["bindings"])
            for record in index["records"]
            for evidence in record["evidence"]
            if evidence.get("fixtureId") == "sf2-map-event-combatant-state-static-v1"
        )
        == 12
    )
    assert normalize_map_event_combatant_state_later_owner_index(index) == (
        normalize_map_event_item_transactions_later_owner_index(predecessor)
    )

    def map20(value: dict[str, object]) -> dict[str, object]:
        return _record(value, "map.data.ms-map20-flag501-zoneevents")

    def jump(value: dict[str, object]) -> dict[str, object]:
        return _record(value, "tech.interfaces.jump-s02")

    for mutator in (
        lambda value: map20(value)["documents"].pop(),
        lambda value: map20(value)["evidence"].append(deepcopy(map20(value)["evidence"][-1])),
        lambda value: jump(value)["addresses"].append(deepcopy(jump(value)["addresses"][-1])),
        lambda value: value["records"][0]["documents"].append("docs/research/unrelated.md"),
        lambda value: value["records"].append(deepcopy(value["records"][0])),
    ):
        broken = deepcopy(index)
        mutator(broken)
        with pytest.raises(ValueError, match="map-event combatant state later-owner"):
            _remove_map_event_combatant_state_later_owner_index_delta(broken)
