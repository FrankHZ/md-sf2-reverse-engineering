from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import REPO_ROOT, repo_path
from sf2tool.source_text import read_upstream_text

INDEX_PATH = repo_path("manifests/research-index.json")
SCHEMA_PATH = repo_path("schemas/research-index.schema.json")
TOOLCHAIN_PATH = repo_path("manifests/toolchain.json")
H3_FIXTURE_ROOT = repo_path("tests/fixtures/h3")


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

        if has_listing:
            match = re.search(
                rf"^(?P<address>[0-9A-F]{{8}})(?:[ \t]+[0-9A-F]{{2,8}})*"
                rf"[ \t]+{re.escape(record['symbol'])}:",
                listing,
                re.MULTILINE,
            )
            if not match:
                raise ValueError(
                    f"indexed symbol {record['symbol']} is absent from the H1 assembler listing"
                )
            listed_address = int(match.group("address"), 16)
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

            for binding in evidence["bindings"]:
                address_id = binding["addressId"]
                if address_id not in addresses:
                    raise ValueError(
                        f"binding in {record_id} refers to missing address ID {address_id!r}"
                    )
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
