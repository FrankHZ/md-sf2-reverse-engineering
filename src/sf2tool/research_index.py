from __future__ import annotations

import hashlib
import importlib
import inspect
import json
import re
from collections.abc import Callable
from copy import deepcopy
from dataclasses import dataclass
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import REPO_ROOT, repo_path
from sf2tool.source_text import read_upstream_text

INDEX_PATH = repo_path("manifests/research-index.json")
SCHEMA_PATH = repo_path("schemas/research-index.schema.json")
TOOLCHAIN_PATH = repo_path("manifests/toolchain.json")
H3_FIXTURE_ROOT = repo_path("tests/fixtures/h3")


@dataclass(frozen=True, slots=True)
class LaterOwnerStep:
    """One exact research-index state transition, newest owner first."""

    owner_id: str
    predecessor_owner_id: str | None
    remover: str
    state_sha256: str
    predecessor_sha256: str


_LATER_OWNER_STEPS = (
    LaterOwnerStep(
        owner_id="sf2-map3-entity142-interactable-reference-static-v1",
        predecessor_owner_id="sf2-map3-original-player-locomotion-animation-runtime-v1",
        remover=(
            "sf2tool.h2.map3_entity142_interactable_reference:"
            "_remove_map3_entity142_interactable_reference_later_owner_index_delta"
        ),
        state_sha256="49B80B7692154290D234A270DFB289AD158AFC96F1EDDE57270C981B438E176A",
        predecessor_sha256=("7F5992C71A615E9BDC1D3FA58ECA752960B16E61B4FF3057BDD170F77A5FC1CE"),
    ),
    LaterOwnerStep(
        owner_id="sf2-map3-original-player-locomotion-animation-runtime-v1",
        predecessor_owner_id="sf2-map3-original-player-reference-frame-static-v1",
        remover=(
            "sf2tool.research_index_locomotion_owner:"
            "_remove_map3_original_player_locomotion_animation_later_owner_index_delta"
        ),
        state_sha256="7F5992C71A615E9BDC1D3FA58ECA752960B16E61B4FF3057BDD170F77A5FC1CE",
        predecessor_sha256=("862D9A6CD7CF860034EF5544D43C68FB4970E295BE590EB69AE99045993295B5"),
    ),
    LaterOwnerStep(
        owner_id="sf2-map3-original-player-reference-frame-static-v1",
        predecessor_owner_id="sf2-map-event-flag-route-selection-static-v1",
        remover=(
            "sf2tool.h2.map3_original_player_reference_frame:"
            "_remove_map3_original_player_reference_frame_later_owner_index_delta"
        ),
        state_sha256="862D9A6CD7CF860034EF5544D43C68FB4970E295BE590EB69AE99045993295B5",
        predecessor_sha256=(
            "70A2A46145FA182EB371D216B54D1F0CF28E24B2555E0194C52B18E88BAD4C0A"
        ),
    ),
    LaterOwnerStep(
        owner_id="sf2-map-event-flag-route-selection-static-v1",
        predecessor_owner_id="sf2-map-event-cross-program-flag-state-static-v1",
        remover=(
            "sf2tool.h2.map_event_flag_route_selection:"
            "_remove_map_event_flag_route_selection_later_owner_index_delta"
        ),
        state_sha256="70A2A46145FA182EB371D216B54D1F0CF28E24B2555E0194C52B18E88BAD4C0A",
        predecessor_sha256=(
            "4F729D50C06D63484565A0DABF15A98F3B092896C7FAF9455DAB884A537DD3FE"
        ),
    ),
    LaterOwnerStep(
        owner_id="sf2-map-event-cross-program-flag-state-static-v1",
        predecessor_owner_id="sf2-map-event-flag-lifecycle-state-static-v1",
        remover=(
            "sf2tool.h2.map_event_cross_program_flag_state:"
            "_remove_map_event_cross_program_flag_state_later_owner_index_delta"
        ),
        state_sha256="4F729D50C06D63484565A0DABF15A98F3B092896C7FAF9455DAB884A537DD3FE",
        predecessor_sha256=(
            "4D526EB33ED5A76D9D69D54E62FC6AB4B412603A16641928566A68200C7C656A"
        ),
    ),
    LaterOwnerStep(
        owner_id="sf2-map-event-flag-lifecycle-state-static-v1",
        predecessor_owner_id="sf2-map-event-scripted-transition-state-static-v1",
        remover=(
            "sf2tool.h2.map_event_flag_lifecycle_state:"
            "_remove_map_event_flag_lifecycle_state_later_owner_index_delta"
        ),
        state_sha256="4D526EB33ED5A76D9D69D54E62FC6AB4B412603A16641928566A68200C7C656A",
        predecessor_sha256=(
            "4241E190B1C52409862AD53412DCCC1F1E8BA3A9868725EC77631851854C6CB1"
        ),
    ),
    LaterOwnerStep(
        owner_id="sf2-map-event-scripted-transition-state-static-v1",
        predecessor_owner_id="sf2-map-event-tactical-base-quote-state-static-v1",
        remover=(
            "sf2tool.h2.map_event_scripted_transition_state:"
            "_remove_map_event_scripted_transition_state_later_owner_index_delta"
        ),
        state_sha256="4241E190B1C52409862AD53412DCCC1F1E8BA3A9868725EC77631851854C6CB1",
        predecessor_sha256=(
            "9A08422491985FF3277A11A1F2BFE2277D3D379FF12681EF835F44AF70CB671D"
        ),
    ),
    LaterOwnerStep(
        owner_id="sf2-map-event-tactical-base-quote-state-static-v1",
        predecessor_owner_id="sf2-map-event-random-battle-state-static-v1",
        remover=(
            "sf2tool.h2.map_event_tactical_base_quote_state:"
            "_remove_map_event_tactical_base_quote_state_later_owner_index_delta"
        ),
        state_sha256="9A08422491985FF3277A11A1F2BFE2277D3D379FF12681EF835F44AF70CB671D",
        predecessor_sha256=(
            "C905CB82A2C310AAAC8A4B40BA7D14BC5750BB4EE9D59AABBF5E68069042630B"
        ),
    ),
    LaterOwnerStep(
        owner_id="sf2-map-event-random-battle-state-static-v1",
        predecessor_owner_id="sf2-map-event-combatant-state-static-v1",
        remover=(
            "sf2tool.h2.map_event_random_battle_state:"
            "_remove_map_event_random_battle_state_later_owner_index_delta"
        ),
        state_sha256="C905CB82A2C310AAAC8A4B40BA7D14BC5750BB4EE9D59AABBF5E68069042630B",
        predecessor_sha256=(
            "9848602E14474EFD9C16FD8E846E14937D09B93F6447E806DEDAE9BE0A17E94A"
        ),
    ),
    LaterOwnerStep(
        owner_id="sf2-map-event-combatant-state-static-v1",
        predecessor_owner_id="sf2-map-event-item-transactions-static-v1",
        remover=(
            "sf2tool.h2.map_event_combatant_state:"
            "_remove_map_event_combatant_state_later_owner_index_delta"
        ),
        state_sha256="9848602E14474EFD9C16FD8E846E14937D09B93F6447E806DEDAE9BE0A17E94A",
        predecessor_sha256=(
            "E987286D1D27BA96DE1A5CF0F3F3179C38CCF19048095865DA4934E4C956ECA7"
        ),
    ),
    LaterOwnerStep(
        owner_id="sf2-map-event-item-transactions-static-v1",
        predecessor_owner_id="sf2-map-event-interaction-state-static-v1",
        remover=(
            "sf2tool.h2.map_event_item_transactions:"
            "_remove_map_event_item_transactions_index_delta"
        ),
        state_sha256="E987286D1D27BA96DE1A5CF0F3F3179C38CCF19048095865DA4934E4C956ECA7",
        predecessor_sha256=(
            "09E54BB6001CFAB23FE3DD034807B4F76EC961931ACEA97F4177F30F96BDE360"
        ),
    ),
    LaterOwnerStep(
        owner_id="sf2-map-event-interaction-state-static-v1",
        predecessor_owner_id=None,
        remover=(
            "sf2tool.h2.map_event_interaction_state:"
            "_remove_map_event_interaction_state_later_owner_index_delta"
        ),
        state_sha256="09E54BB6001CFAB23FE3DD034807B4F76EC961931ACEA97F4177F30F96BDE360",
        predecessor_sha256=(
            "E8B95158841944757D09EFA4AE63B58E451659475A2C6A0E991E32331ED8B787"
        ),
    ),
)

_LaterOwnerRemover = Callable[[dict[str, Any]], dict[str, Any]]
_LaterOwnerResolver = Callable[[LaterOwnerStep], _LaterOwnerRemover]


def _canonical_index_sha256(index: dict[str, Any]) -> str:
    payload = json.dumps(
        index, ensure_ascii=False, sort_keys=True, separators=(",", ":")
    ).encode("utf-8") + b"\n"
    return hashlib.sha256(payload).hexdigest().upper()


def _validate_later_owner_steps(
    steps: tuple[LaterOwnerStep, ...],
) -> dict[str, LaterOwnerStep]:
    if not steps or any(not isinstance(step, LaterOwnerStep) for step in steps):
        raise ValueError("research-index later-owner registry shape drift")
    owners = [step.owner_id for step in steps]
    removers = [step.remover for step in steps]
    if len(owners) != len(set(owners)):
        raise ValueError("duplicate research-index later-owner ID")
    if len(removers) != len(set(removers)):
        raise ValueError("duplicate research-index later-owner remover")
    by_id = {step.owner_id: step for step in steps}
    digest_pattern = re.compile(r"[0-9A-F]{64}")
    for step in steps:
        module_name, separator, attribute = step.remover.partition(":")
        if (
            not step.owner_id
            or not separator
            or not module_name
            or not attribute
            or digest_pattern.fullmatch(step.state_sha256) is None
            or digest_pattern.fullmatch(step.predecessor_sha256) is None
        ):
            raise ValueError("research-index later-owner registry entry drift")
        if step.predecessor_owner_id is not None and step.predecessor_owner_id not in by_id:
            raise ValueError("missing research-index later-owner predecessor")

    referenced = {
        step.predecessor_owner_id
        for step in steps
        if step.predecessor_owner_id is not None
    }
    heads = set(owners) - referenced
    terminals = [step for step in steps if step.predecessor_owner_id is None]
    if len(heads) != 1 or len(terminals) != 1:
        raise ValueError("research-index later-owner registry is not one closed chain")

    ordered: list[str] = []
    current_id: str | None = next(iter(heads))
    while current_id is not None:
        if current_id in ordered:
            raise ValueError("research-index later-owner registry cycle")
        ordered.append(current_id)
        current_id = by_id[current_id].predecessor_owner_id
    if len(ordered) != len(steps):
        raise ValueError("research-index later-owner registry is disconnected")
    if ordered != owners:
        raise ValueError("research-index later-owner registry order drift")
    for current, predecessor in zip(steps, steps[1:], strict=False):
        if (
            current.predecessor_owner_id != predecessor.owner_id
            or current.predecessor_sha256 != predecessor.state_sha256
        ):
            raise ValueError("research-index later-owner registry continuity drift")
    return by_id


def _resolve_later_owner_remover(step: LaterOwnerStep) -> _LaterOwnerRemover:
    module_name, _separator, attribute = step.remover.partition(":")
    module = importlib.import_module(module_name)
    if getattr(module, "ID", None) != step.owner_id:
        raise ValueError("research-index later-owner module identity drift")
    remover = getattr(module, attribute, None)
    if not callable(remover):
        raise ValueError("research-index later-owner remover is unavailable")
    try:
        inspect.signature(remover).bind({})
    except TypeError as exc:
        raise ValueError("research-index later-owner remover signature drift") from exc
    return remover


def _normalize_current_index(
    index: dict[str, Any],
    *,
    owner_id: str,
    include_owner: bool,
    steps: tuple[LaterOwnerStep, ...] = _LATER_OWNER_STEPS,
    resolver: _LaterOwnerResolver = _resolve_later_owner_remover,
) -> dict[str, Any]:
    owners = _validate_later_owner_steps(steps)
    if owner_id not in owners:
        raise ValueError(f"unknown research-index later owner: {owner_id}")
    normalized = deepcopy(index)
    if _canonical_index_sha256(normalized) != steps[0].state_sha256:
        raise ValueError("research-index later-owner head state drift")
    for step in steps:
        if step.owner_id == owner_id and not include_owner:
            return normalized
        remover = resolver(step)
        predecessor = remover(normalized)
        if not isinstance(predecessor, dict) or predecessor is normalized:
            raise ValueError("research-index later-owner remover result drift")
        if _canonical_index_sha256(predecessor) != step.predecessor_sha256:
            raise ValueError("research-index later-owner predecessor state drift")
        normalized = predecessor
        if step.owner_id == owner_id:
            return normalized
    raise AssertionError("validated research-index later-owner target was not reached")


def _normalize_current_index_to_owner_state(
    index: dict[str, Any], *, owner_id: str
) -> dict[str, Any]:
    """Return the exact registered state in which ``owner_id`` is still present."""
    return _normalize_current_index(index, owner_id=owner_id, include_owner=False)


def normalize_current_index_to_owner_predecessor(
    index: dict[str, Any], *, owner_id: str
) -> dict[str, Any]:
    """Strictly normalize the current index through one target owner's predecessor."""
    return _normalize_current_index(index, owner_id=owner_id, include_owner=True)


def listing_symbol_addresses(listing: str) -> dict[str, int]:
    addresses: dict[str, int] = {}
    for match in re.finditer(
        r"^(?P<address>[0-9A-F]{8})(?:[ \t]+[0-9A-F]{2,8})*"
        r"[ \t]+(?P<symbol>[A-Za-z_][A-Za-z0-9_]*):",
        listing,
        re.MULTILINE,
    ):
        symbol = match.group("symbol")
        address = int(match.group("address"), 16)
        previous = addresses.setdefault(symbol, address)
        if previous != address:
            raise ValueError(
                f"H1 assembler listing defines {symbol} at conflicting addresses: "
                f"0x{previous:X}, 0x{address:X}"
            )
    return addresses


def _nested_value(value: Any, field: str) -> Any:
    for segment in field.split("."):
        if not isinstance(value, dict) or segment not in value:
            raise ValueError(f"missing fixture field: {field}")
        value = value[segment]
    return value


def _repo_file(relative: str, *, owner: str) -> Path:
    path = repo_path(relative)
    if not path.is_file():
        raise ValueError(f"missing {owner}: {relative}")
    return path


def verify_index(upstream_path: Path | None = None) -> dict[str, Any]:
    index = load_json(INDEX_PATH)
    validate_json(index, SCHEMA_PATH, owner="research index")
    toolchain = load_json(TOOLCHAIN_PATH)
    expected_upstream = toolchain["sf2disasm"]
    if (
        index["upstream"]["repository"] != expected_upstream["repository"]
        or index["upstream"]["commit"] != expected_upstream["commit"]
    ):
        raise ValueError("research index provenance does not match manifests/toolchain.json")

    upstream_path = upstream_path.resolve() if upstream_path else None
    source_root = upstream_path / index["upstream"]["sourceRoot"] if upstream_path else None
    listing_path = upstream_path / index["upstream"]["listingPath"] if upstream_path else None
    has_sources = bool(source_root and source_root.is_dir())
    has_listing = bool(listing_path and listing_path.is_file())
    listing = listing_path.read_text(encoding="utf-8") if has_listing and listing_path else ""
    listing_addresses = listing_symbol_addresses(listing) if has_listing else {}

    record_ids: set[str] = set()
    fixture_ids: dict[str, str] = {}
    fixture_objects: dict[str, dict[str, Any]] = {}
    fixture_bindings: set[str] = set()
    documents: set[str] = set()
    contracts: set[str] = set()
    binding_count = 0

    for record in index["records"]:
        record_id = record["id"]
        if record_id in record_ids:
            raise ValueError(f"duplicate research record ID: {record_id}")
        record_ids.add(record_id)
        listing_domain = record.get("listingDomain", "h1-68000")

        addresses: dict[str, dict[str, Any]] = {}
        symbol_addresses = [
            address
            for address in record["addresses"]
            if address["space"] == "rom" and address["kind"] == "symbol"
        ]
        if len(symbol_addresses) != 1:
            raise ValueError(f"{record_id} must define exactly one ROM symbol address")
        for address in record["addresses"]:
            if address["id"] in addresses:
                raise ValueError(f"duplicate address ID {address['id']!r} in {record_id}")
            addresses[address["id"]] = address

        if has_sources and source_root:
            source_path = source_root / record["sourcePath"]
            if not source_path.is_file():
                raise ValueError(f"missing indexed upstream source: {record['sourcePath']}")
            source_text = read_upstream_text(source_path)
            if not re.search(rf"^{re.escape(record['symbol'])}:", source_text, re.MULTILINE):
                raise ValueError(
                    f"indexed symbol {record['symbol']} is absent from {record['sourcePath']}"
                )

        if listing_domain == "z80-music-bank":
            if not re.fullmatch(
                r"data/sound/musicbank[01]/music\d+\.asm", record["sourcePath"]
            ):
                raise ValueError(f"invalid Z80 music-bank source path in {record_id}")
            if any(
                evidence["fixture"] != "tests/fixtures/h2/sound-data-static-v1.json"
                or evidence["verifier"] != "src/sf2tool/h2/sound_data.py"
                for evidence in record["evidence"]
            ):
                raise ValueError(f"invalid Z80 music-bank evidence owner in {record_id}")
            if not 0x1F0000 <= symbol_addresses[0]["value"] < 0x200000:
                raise ValueError(f"Z80 music-bank ROM address is outside both banks in {record_id}")
        elif has_listing:
            if record["symbol"] not in listing_addresses:
                raise ValueError(
                    f"indexed symbol {record['symbol']} is absent from the H1 assembler listing"
                )
            listed_address = listing_addresses[record["symbol"]]
            indexed_address = symbol_addresses[0]["value"]
            if listed_address != indexed_address:
                raise ValueError(
                    f"H1 listing address drift for {record['symbol']}: "
                    f"index 0x{indexed_address:X}, listing 0x{listed_address:X}"
                )

        for document in record["documents"]:
            _repo_file(document, owner="research document")
            documents.add(document)
        for contract in record.get("designContracts", []):
            _repo_file(contract, owner="design contract")
            contracts.add(contract)

        for evidence in record["evidence"]:
            fixture_relative = evidence["fixture"]
            fixture_path = _repo_file(fixture_relative, owner="fixture")
            _repo_file(evidence["verifier"], owner="verifier")
            fixture = fixture_objects.setdefault(fixture_relative, load_json(fixture_path))
            if fixture["id"] != evidence["fixtureId"]:
                raise ValueError(
                    f"fixture ID drift at {fixture_relative}: "
                    f"index {evidence['fixtureId']!r}, fixture {fixture['id']!r}"
                )
            if fixture["romSha256"] != index["rom"]["sha256"]:
                raise ValueError(f"ROM identity drift at {fixture_relative}")
            previous_id = fixture_ids.setdefault(fixture_relative, evidence["fixtureId"])
            if previous_id != evidence["fixtureId"]:
                raise ValueError(f"conflicting fixture IDs indexed for {fixture_relative}")

            evidence_address_ids: set[str] = set()
            for binding in evidence["bindings"]:
                address_id = binding["addressId"]
                if address_id not in addresses:
                    raise ValueError(
                        f"binding in {record_id} refers to missing address ID {address_id!r}"
                    )
                if address_id in evidence_address_ids:
                    raise ValueError(
                        f"duplicate evidence address ID {address_id!r} in {record_id}"
                    )
                evidence_address_ids.add(address_id)
                fixture_value = _nested_value(fixture, binding["fixtureField"])
                index_value = addresses[address_id]["value"]
                if fixture_value != index_value:
                    raise ValueError(
                        f"address drift at {fixture_relative}::{binding['fixtureField']}: "
                        f"index {index_value}, fixture {fixture_value}"
                    )
                fixture_bindings.add(f"{fixture_relative}::{binding['fixtureField']}")
                binding_count += 1

    h3_fixtures = sorted(H3_FIXTURE_ROOT.glob("*.json"))
    for fixture_path in h3_fixtures:
        fixture_relative = fixture_path.relative_to(REPO_ROOT).as_posix()
        if fixture_relative not in fixture_ids:
            raise ValueError(f"H3 fixture is missing from the research index: {fixture_relative}")
        fixture = fixture_objects[fixture_relative]
        for section_name in ("function", "ram"):
            section = fixture.get(section_name)
            if not isinstance(section, dict):
                continue
            for field in section:
                if field.endswith("Address"):
                    key = f"{fixture_relative}::{section_name}.{field}"
                    if key not in fixture_bindings:
                        raise ValueError(
                            f"fixture address is not bound by the research index: {key}"
                        )

    indexed_h3_count = sum(path.startswith("tests/fixtures/h3/") for path in fixture_ids)
    indexed_h2_count = sum(path.startswith("tests/fixtures/h2/") for path in fixture_ids)
    indexed_source_paths = {record["sourcePath"] for record in index["records"]}
    h1_records = [
        record
        for record in index["records"]
        if record.get("listingDomain", "h1-68000") == "h1-68000"
    ]
    alternate_records = [
        record
        for record in index["records"]
        if record.get("listingDomain", "h1-68000") != "h1-68000"
    ]
    return {
        "Index": "manifests/research-index.json",
        "Records": len(index["records"]),
        "Confirmed": sum(record["status"] == "confirmed" for record in index["records"]),
        "H2Fixtures": indexed_h2_count,
        "H3Fixtures": indexed_h3_count,
        "H3FixtureFiles": len(h3_fixtures),
        "AddressBindings": binding_count,
        "IndexedCodeFiles": sum(path.startswith("code/") for path in indexed_source_paths),
        "IndexedDataFiles": sum(path.startswith("data/") for path in indexed_source_paths),
        "H1ListingRecords": len(h1_records),
        "AlternateListingRecords": len(alternate_records),
        "Z80MusicBankRecords": sum(
            record.get("listingDomain") == "z80-music-bank" for record in index["records"]
        ),
        "ResearchDocuments": len(documents),
        "DesignContracts": len(contracts),
        "UpstreamSourcesChecked": has_sources,
        "H1ListingChecked": has_listing,
        "Status": "PASS",
    }


def query_index(
    *,
    query: str | None = None,
    subsystem: str | None = None,
    status: str | None = None,
    fixture: str | None = None,
) -> list[dict[str, Any]]:
    records = load_json(INDEX_PATH)["records"]
    if query:
        query_folded = query.casefold()
        records = [
            record
            for record in records
            if any(
                query_folded in record[field].casefold() for field in ("id", "symbol", "subsystem")
            )
        ]
    if subsystem:
        subsystem_folded = subsystem.casefold()
        records = [
            record
            for record in records
            if record["subsystem"].casefold().startswith(subsystem_folded)
        ]
    if status:
        records = [record for record in records if record["status"] == status]
    if fixture:
        fixture_folded = fixture.casefold()
        records = [
            record
            for record in records
            if any(fixture_folded in item["fixture"].casefold() for item in record["evidence"])
        ]
    return records


def index_summary(records: list[dict[str, Any]]) -> dict[str, Any]:
    fixtures = {evidence["fixture"] for record in records for evidence in record["evidence"]}
    return {
        "Records": len(records),
        "Confirmed": sum(record["status"] == "confirmed" for record in records),
        "Inferred": sum(record["status"] == "inferred" for record in records),
        "Unknown": sum(record["status"] == "unknown" for record in records),
        "Fixtures": len(fixtures),
        "Subsystems": sorted({record["subsystem"] for record in records}),
    }


def index_rows(records: list[dict[str, Any]]) -> list[dict[str, Any]]:
    rows = []
    for record in records:
        entry = next(
            address
            for address in record["addresses"]
            if address["space"] == "rom" and address["kind"] == "symbol"
        )
        rows.append(
            {
                "Id": record["id"],
                "Subsystem": record["subsystem"],
                "Status": record["status"],
                "Symbol": record["symbol"],
                "Entry": f"0x{entry['value']:X}",
                "Fixtures": len({item["fixture"] for item in record["evidence"]}),
                "Documents": len(record["documents"]),
            }
        )
    return rows
