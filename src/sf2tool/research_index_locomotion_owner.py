"""Exact research-index delta removal for the accepted Map 3 locomotion owner."""

from __future__ import annotations

import hashlib
import json
from copy import deepcopy
from typing import Any

ID = "sf2-map3-original-player-locomotion-animation-runtime-v1"

_INDEX_FIXTURE = "tests/fixtures/h3/map3-original-player-locomotion-animation-runtime-v1.json"
_INDEX_DOCUMENT = "docs/research/map3-original-player-locomotion-animation.md"
_INDEX_VERIFIER = "src/sf2tool/h3/map3_original_player_locomotion_animation.py"
_INDEX_BINDINGS = {
    "scripting.entity.entityscriptengine-1": (
        ("entry", "sourceContext.functions.vintUpdateSprites"),
        ("half-0", "sourceContext.functions.spriteHalf0"),
        ("half-1", "sourceContext.functions.spriteHalf1"),
        ("counter-after", "sourceContext.functions.spriteCounterAfter"),
    ),
    "gameflow.exploration.loop": (("wait-for-event", "sourceContext.functions.waitForEvent"),),
    "entity.actions.update-core": (
        ("entry", "sourceContext.functions.updateEntityData"),
        ("return", "sourceContext.functions.updateEntityDataReturn"),
        ("entity-data", "sourceContext.ram.ENTITY_DATA"),
    ),
}
_INDEX_ADDRESSES = {
    "scripting.entity.entityscriptengine-1": (
        {
            "id": "half-0",
            "space": "rom",
            "kind": "observation",
            "value": 19764,
            "description": "VInt_UpdateSprites first-half selection instruction",
        },
        {
            "id": "half-1",
            "space": "rom",
            "kind": "observation",
            "value": 19770,
            "description": "VInt_UpdateSprites second-half selection instruction",
        },
        {
            "id": "counter-after",
            "space": "rom",
            "kind": "observation",
            "value": 19804,
            "description": "VInt_UpdateSprites post-increment counter observation seam",
        },
    ),
    "entity.actions.update-core": (
        {
            "id": "return",
            "space": "rom",
            "kind": "observation",
            "value": 24474,
            "description": "UpdateEntityData return observation seam",
        },
    ),
}
_PREDECESSOR_INDEX_SHA256 = "54F3A89A9578BAE26FAB2D7259BA927D88C2756F7137F376C26B7B98161A5FE7"


def _index_digest(value: dict[str, Any]) -> str:
    payload = (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")
    return hashlib.sha256(payload).hexdigest().upper()


def _remove_map3_original_player_locomotion_animation_later_owner_index_delta(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Remove exactly this accepted runtime owner's index delta."""
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list) or any(not isinstance(row, dict) for row in records):
        raise ValueError("Map 3 player locomotion index record shape drift")
    seen: set[str] = set()
    for record in records:
        record_id = record.get("id")
        evidence = record.get("evidence")
        documents = record.get("documents")
        addresses = record.get("addresses")
        if (
            not isinstance(evidence, list)
            or not isinstance(documents, list)
            or not isinstance(addresses, list)
        ):
            raise ValueError("Map 3 player locomotion index field drift")
        markers = [
            row
            for row in evidence
            if isinstance(row, dict)
            and (
                row.get("fixtureId") == ID
                or row.get("fixture") == _INDEX_FIXTURE
                or row.get("verifier") == _INDEX_VERIFIER
            )
        ]
        document_count = documents.count(_INDEX_DOCUMENT)
        expected_bindings = _INDEX_BINDINGS.get(record_id)
        if expected_bindings is None:
            if markers or document_count:
                raise ValueError("Map 3 player locomotion index unknown-record drift")
            continue
        expected = {
            "level": "H3",
            "fixture": _INDEX_FIXTURE,
            "fixtureId": ID,
            "verifier": _INDEX_VERIFIER,
            "bindings": [
                {"addressId": address_id, "fixtureField": fixture_field}
                for address_id, fixture_field in expected_bindings
            ],
        }
        expected_addresses = list(_INDEX_ADDRESSES.get(str(record_id), ()))
        if markers != [expected] or evidence[-1] != expected:
            raise ValueError("Map 3 player locomotion index evidence drift")
        if document_count != 1 or documents[-1] != _INDEX_DOCUMENT:
            raise ValueError("Map 3 player locomotion index document drift")
        if expected_addresses and addresses[-len(expected_addresses) :] != expected_addresses:
            raise ValueError("Map 3 player locomotion index address drift")
        evidence.remove(expected)
        documents.remove(_INDEX_DOCUMENT)
        for address in expected_addresses:
            addresses.remove(address)
        seen.add(str(record_id))
    if seen != set(_INDEX_BINDINGS):
        raise ValueError("Map 3 player locomotion index denominator drift")
    if _index_digest(normalized) != _PREDECESSOR_INDEX_SHA256:
        raise ValueError("Map 3 player locomotion predecessor index drift")
    return normalized
