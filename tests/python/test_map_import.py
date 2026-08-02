from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h2 import map_import
from sf2tool.h2.map_import import _decode_source_table

_EVENT_RECORDS = {
    "entityEvents": {
        "address": 0x1000,
        "kind": "default",
        "relativeOffset": 4,
        "resolvedTargetAddress": 0x1004,
        "entity": 253,
        "flags": 0,
    },
    "zoneEvents": {
        "address": 0x2000,
        "kind": "specific",
        "relativeOffset": 4,
        "resolvedTargetAddress": 0x2004,
        "x": 14,
        "y": 12,
    },
    "itemEvents": {
        "address": 0x3000,
        "kind": "default",
        "relativeOffset": 6,
        "resolvedTargetAddress": 0x3006,
        "x": 253,
        "y": 0,
        "facing": 0,
        "item": 1,
    },
}


def _event_contract(category: str, record: dict[str, object]) -> dict[str, object]:
    symbol = f"{category}-handler"
    return {
        "categories": {
            category: {
                "tables": [{"symbol": symbol, "records": [record]}],
                "sourceFiles": [
                    {"symbol": symbol, "address": record["address"], "directReturnStub": False}
                ],
            }
        }
    }


def _setup_contracts(init_source: dict[str, object]) -> dict[str, object]:
    upstream = {"commit": "pinned-commit"}
    return {
        "setup": {"upstream": upstream, "routes": [], "pointerTables": []},
        "entities": {"upstream": upstream, "lists": []},
        "events": {
            "upstream": upstream,
            "categories": {
                name: {"tables": [], "sourceFiles": []}
                for name in ("entityEvents", "zoneEvents", "itemEvents")
            },
        },
        "descriptions": {"upstream": upstream, "sourceFiles": []},
        "init": {
            "upstream": upstream,
            "sourceFiles": [init_source],
            "primarySourceBodies": [
                {
                    "sourceOwnerSymbol": "ms_Test_InitFunction",
                    "operations": [
                        {
                            "index": 0,
                            "labels": ["ms_Test_InitFunction"],
                            "opcode": "moveq",
                            "operandText": "#0,d0",
                            "branchTargetSymbol": None,
                            "branchTargetAddress": None,
                            "localBranchTargetIndex": None,
                        },
                        {
                            "index": 1,
                            "labels": ["ms_Test_InitFunction", "@Call"],
                            "opcode": "jsr",
                            "operandText": "Target",
                            "branchTargetSymbol": None,
                            "branchTargetAddress": None,
                            "localBranchTargetIndex": None,
                        },
                        {
                            "index": 2,
                            "labels": ["@Loop"],
                            "opcode": "bne.s",
                            "operandText": "@Call",
                            "branchTargetSymbol": "@Call",
                            "branchTargetAddress": None,
                            "localBranchTargetIndex": 1,
                        },
                    ],
                }
            ],
            "embeddedPrograms": [],
            "scriptTargetCounts": {},
        },
        "scripts": {
            "upstream": upstream,
            "programs": [],
            "standaloneOwnedInitScriptTargets": [],
        },
    }


def _init_source(*, direct_call_targets: list[str]) -> dict[str, object]:
    return {
        "symbol": "ms_Test_InitFunction",
        "address": 0x1234,
        "sourceOwnerSymbol": "ms_Test_InitFunction",
        "directReturnStub": False,
        "bodySha256": "A" * 64,
        "firstOperationIndex": 1,
        "lastOperationIndex": 2,
        "operationIndices": [1, 2],
        "scriptTargets": ["cs_Test"],
        "directCallTargets": direct_call_targets,
    }


def _patch_setup_contracts(
    monkeypatch: pytest.MonkeyPatch, contracts: dict[str, object]
) -> None:
    monkeypatch.setattr(
        map_import, "build_map_setup_contract", lambda *_: contracts["setup"]
    )
    monkeypatch.setattr(
        map_import, "build_map_entities_contract", lambda *_: contracts["entities"]
    )
    monkeypatch.setattr(
        map_import, "build_map_events_contract", lambda *_: contracts["events"]
    )
    monkeypatch.setattr(
        map_import, "build_map_descriptions_contract", lambda *_: contracts["descriptions"]
    )
    monkeypatch.setattr(map_import, "build_map_init_contract", lambda *_: contracts["init"])
    monkeypatch.setattr(
        map_import, "build_map_scripts_inventory", lambda *_: contracts["scripts"]
    )


@pytest.mark.parametrize("category", sorted(_EVENT_RECORDS))
def test_event_handler_resources_keep_exact_canonical_fields_not_producer_provenance(
    category: str,
) -> None:
    record = {**_EVENT_RECORDS[category], "sourcePath": "new-producer-provenance.asm"}

    handlers = map_import._event_handler_resources(_event_contract(category, record), category)

    assert handlers == [
        {
            "id": f"{category}-handler",
            "address": _EVENT_RECORDS[category]["address"],
            "kind": "table",
            "records": [_EVENT_RECORDS[category]],
        }
    ]


@pytest.mark.parametrize(
    ("category", "required_field"),
    [
        ("entityEvents", "flags"),
        ("zoneEvents", "x"),
        ("itemEvents", "facing"),
    ],
)
def test_event_handler_resources_rejects_missing_or_renamed_canonical_fields(
    category: str, required_field: str
) -> None:
    record = deepcopy(_EVENT_RECORDS[category])
    record[f"renamed{required_field.title()}"] = record.pop(required_field)

    with pytest.raises(KeyError, match=required_field):
        map_import._event_handler_resources(_event_contract(category, record), category)


def test_setup_resources_maps_accepted_direct_call_targets_to_canonical_call_targets(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    init_source = _init_source(direct_call_targets=["DirectTarget", "j_Interface"])
    init_source["callTargets"] = ["stale-consumer-field"]
    _patch_setup_contracts(monkeypatch, _setup_contracts(init_source))

    resources, routes, facts = map_import._setup_resources(
        Path("rom.bin"), Path("upstream"), "pinned-commit"
    )

    assert resources["initFunctions"] == [
        {
            "id": "ms_Test_InitFunction",
            "address": 0x1234,
            "kind": "operationList",
            "bodySha256": "A" * 64,
            "scriptTargets": ["cs_Test"],
            "callTargets": ["DirectTarget", "j_Interface"],
            "operations": [
                {
                    "index": 0,
                    "labels": ["@Call"],
                    "opcode": "jsr",
                    "operandText": "Target",
                    "branchTargetSymbol": None,
                    "branchTargetAddress": None,
                    "localBranchTargetIndex": None,
                },
                {
                    "index": 1,
                    "labels": ["@Loop"],
                    "opcode": "bne.s",
                    "operandText": "@Call",
                    "branchTargetSymbol": "@Call",
                    "branchTargetAddress": None,
                    "localBranchTargetIndex": 0,
                },
            ],
        }
    ]
    assert routes == {}
    assert facts["initOperationCount"] == 2


@pytest.mark.parametrize("replacement_key", ["callTargets", "directCallTargetSymbols"])
def test_setup_resources_rejects_missing_or_renamed_accepted_direct_call_targets(
    monkeypatch: pytest.MonkeyPatch, replacement_key: str
) -> None:
    init_source = _init_source(direct_call_targets=["DirectTarget"])
    init_source[replacement_key] = init_source.pop("directCallTargets")
    _patch_setup_contracts(monkeypatch, _setup_contracts(deepcopy(init_source)))

    with pytest.raises(KeyError, match="directCallTargets"):
        map_import._setup_resources(Path("rom.bin"), Path("upstream"), "pinned-commit")


def test_setup_resources_rejects_noncontiguous_init_profile_operation_indices(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    init_source = _init_source(direct_call_targets=[])
    init_source["operationIndices"] = [0, 2]
    _patch_setup_contracts(monkeypatch, _setup_contracts(init_source))

    with pytest.raises(ValueError, match="profile operation boundary drift"):
        map_import._setup_resources(Path("rom.bin"), Path("upstream"), "pinned-commit")


def test_setup_resources_rejects_init_profile_source_operation_index_drift(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    init_source = _init_source(direct_call_targets=[])
    contracts = _setup_contracts(init_source)
    source_operations = contracts["init"]["primarySourceBodies"][0]["operations"]
    source_operations[1]["index"] = 3
    _patch_setup_contracts(monkeypatch, contracts)

    with pytest.raises(ValueError, match="source operation boundary drift"):
        map_import._setup_resources(Path("rom.bin"), Path("upstream"), "pinned-commit")


def test_source_table_decoders_preserve_raw_values() -> None:
    flag = _decode_source_table("flagEvents", bytes.fromhex("0348002602030E0AFFFF"), 1, False)
    assert flag == [
        {
            "flag": 840,
            "source": {"x": 0, "y": 38},
            "size": {"width": 2, "height": 3},
            "destination": {"x": 14, "y": 10},
        }
    ]

    warp = _decode_source_table("warpEvents", bytes.fromhex("0E0B12010A1F0000FFFF"), 1, False)
    assert warp[0]["scrollMode"] == 0x12
    assert warp[0]["retainsCoordinates"] is True
    assert warp[0]["scrollDirection"] == 2
    assert warp[0]["reserved"] == 0


def test_item_table_terminator_excludes_trailing_rts() -> None:
    item = _decode_source_table("otherItems", bytes.fromhex("181A8601FFFF4E75"), 1, True)
    assert item == [{"x": 24, "y": 26, "flag": 134, "item": 1}]


def test_animation_header_names_cached_tile_count_not_speed() -> None:
    animation = _decode_source_table(
        "animations",
        bytes.fromhex("002E00200000001001700014FFFF"),
        1,
        False,
    )

    assert animation == {
        "tileset": 46,
        "cachedTileCount": 32,
        "entries": [
            {
                "replacementStartTile": 0,
                "tileCount": 16,
                "targetStartTile": 0x170,
                "counter": 20,
            }
        ],
    }
