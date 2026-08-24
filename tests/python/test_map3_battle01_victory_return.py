from __future__ import annotations

import copy
import json
from pathlib import Path

import pytest
from jsonschema import Draft7Validator

from sf2tool.h2 import map3_battle01_victory_return as victory_return

ROOT = Path(__file__).resolve().parents[2]
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"


def test_scoped_source_inventory_and_h1_rom_anchors_are_complete() -> None:
    text, identities = victory_return._read_source_surface(UPSTREAM / "disasm")
    assert len(text) == len(identities) == 16
    assert [row["path"] for row in identities] == list(victory_return._SOURCE_SURFACE)
    anchors = victory_return._anchor_projection(
        (UPSTREAM / "build/sf2build-h1.bin").read_bytes(), ROM.read_bytes()
    )
    assert len(anchors) == 46
    assert anchors[0]["address"] == 0x23BFC
    assert anchors[-1]["address"] == 0x257C0


def test_battle01_after_battle_program_has_the_closed_public_corpus() -> None:
    text = victory_return._read_source_surface(UPSTREAM / "disasm")[0]
    program = victory_return._validate_source_contract(text)
    assert len(program["operations"]) == 80
    assert program["commandForms"] == list(victory_return._PROGRAM_FORMS)
    assert program["operations"][0]["operands"] == ["2305"]
    assert program["operations"][-1]["command"] == "cscEntitiesEnd"


def test_fixture_is_closed_and_has_the_exact_static_denominators() -> None:
    fixture = json.loads(victory_return.FIXTURE.read_text(encoding="utf-8"))
    schema = json.loads(victory_return.SCHEMA.read_text(encoding="utf-8"))
    assert list(fixture) == [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "system",
        "summary",
        "retainedR3d",
        "retainedOwners",
        "sourceContext",
        "victoryReturnSpine",
        "unknowns",
    ]
    assert fixture["summary"] == {
        "sourceFiles": 16,
        "h1RomAnchors": 46,
        "indexObjects": 11,
        "indexBindings": 29,
        "unknowns": 50,
        "programOperations": 80,
        "programCommandForms": 27,
    }
    assert all(value == "Unknown" for value in fixture["unknowns"].values())
    assert len(fixture["unknowns"]) == 50
    assert not list(Draft7Validator(schema).iter_errors(fixture))


def test_structural_schema_rejects_nested_runtime_or_extra_fields() -> None:
    fixture = json.loads(victory_return.FIXTURE.read_text(encoding="utf-8"))
    schema = json.loads(victory_return.SCHEMA.read_text(encoding="utf-8"))
    malformed = copy.deepcopy(fixture)
    malformed["sourceContext"]["runtime"] = True
    assert list(Draft7Validator(schema).iter_errors(malformed))
    malformed = copy.deepcopy(fixture)
    malformed["victoryReturnSpine"]["h3"] = "forbidden"
    assert list(Draft7Validator(schema).iter_errors(malformed))


@pytest.mark.parametrize(
    "mutation",
    (
        lambda value: value["victoryReturnSpine"]["functionAddresses"].update({"Unexpected": 1}),
        lambda value: value["victoryReturnSpine"]["victoryBody"].update({"unexpected": 1}),
        lambda value: value["unknowns"].__setitem__(
            "wrongUnknown", value["unknowns"].pop("naturalContinuity")
        ),
        lambda value: value["sourceContext"].__setitem__(
            "sourceIdentities", list(reversed(value["sourceContext"]["sourceIdentities"]))
        ),
        lambda value: value["victoryReturnSpine"]["battle01AfterBattleProgram"][
            "commandForms"
        ].__setitem__(0, "wrongForm"),
        lambda value: value["sourceContext"]["h1RomAnchors"][0].__setitem__("width", 3),
        lambda value: value["victoryReturnSpine"]["ownerRecordIds"].reverse(),
    ),
    ids=(
        "function-extra",
        "body-extra",
        "unknown-key",
        "source-order",
        "command-form",
        "anchor-value",
        "owner-order",
    ),
)
def test_schema_rejects_every_closed_victory_return_shape(mutation: object) -> None:
    fixture = json.loads(victory_return.FIXTURE.read_text(encoding="utf-8"))
    schema = json.loads(victory_return.SCHEMA.read_text(encoding="utf-8"))
    mutation(fixture)  # type: ignore[operator]
    assert list(Draft7Validator(schema).iter_errors(fixture))


@pytest.mark.parametrize("identifier,address,width,_end", victory_return._ANCHORS)
def test_every_h1_rom_anchor_rejects_a_single_byte_mismatch(
    identifier: str, address: int, width: int, _end: int | None
) -> None:
    h1 = bytearray((UPSTREAM / "build/sf2build-h1.bin").read_bytes())
    h1[address + width - 1] ^= 0x01
    with pytest.raises(ValueError, match="anchor drift"):
        victory_return._anchor_projection(bytes(h1), ROM.read_bytes())


@pytest.mark.parametrize("operation_id", range(80))
def test_every_program_operation_mutation_is_rejected_before_golden(operation_id: int) -> None:
    text = victory_return._read_source_surface(UPSTREAM / "disasm")[0]
    program = victory_return._validate_source_contract(text)
    malformed = copy.deepcopy(program)
    malformed["operations"][operation_id]["operands"].append("near-miss")
    with pytest.raises(ValueError, match="program corpus contract drift"):
        victory_return._parse_h1((UPSTREAM / "build/sf2build-h1.bin").read_bytes(), malformed)


@pytest.mark.parametrize("form_id", range(27))
def test_every_program_form_and_boundary_is_rejected_before_golden(form_id: int) -> None:
    text = victory_return._read_source_surface(UPSTREAM / "disasm")[0]
    program = victory_return._validate_source_contract(text)
    malformed = copy.deepcopy(program)
    malformed["commandForms"][form_id] = "near-miss"
    with pytest.raises(ValueError, match="program corpus contract drift"):
        victory_return._parse_h1((UPSTREAM / "build/sf2build-h1.bin").read_bytes(), malformed)
    malformed = copy.deepcopy(program)
    malformed["operations"].pop()
    with pytest.raises(ValueError, match="program corpus contract drift"):
        victory_return._parse_h1((UPSTREAM / "build/sf2build-h1.bin").read_bytes(), malformed)


def test_source_use_sites_reject_comments_near_misses_and_mainloop_order() -> None:
    text = victory_return._read_source_surface(UPSTREAM / "disasm")[0]
    malformed = copy.deepcopy(text)
    malformed[victory_return._SOURCE_SURFACE[0]] = malformed[
        victory_return._SOURCE_SURFACE[0]
    ].replace("moveq   #1,d4", "moveq   #2,d4 ; moveq #1,d4")
    with pytest.raises(ValueError, match="victory body"):
        victory_return._validate_source_contract(malformed)
    malformed = copy.deepcopy(text)
    malformed[victory_return._SOURCE_SURFACE[13]] = malformed[
        victory_return._SOURCE_SURFACE[13]
    ].replace(
        "bsr.w   SwitchMap       ; Check table",
        "jsr     j_ExplorationLoop       ; Check table",
    )
    with pytest.raises(ValueError, match="MainLoop return"):
        victory_return._validate_source_contract(malformed)


def test_index_delta_has_exact_eleven_objects_eighteen_addresses_and_twenty_nine_bindings() -> None:
    index = json.loads((ROOT / "manifests/research-index.json").read_text(encoding="utf-8"))
    evidence = [
        (record, item)
        for record in index["records"]
        for item in record["evidence"]
        if item.get("fixtureId") == victory_return.ID
    ]
    assert len(evidence) == 11
    bindings = [binding for _, item in evidence for binding in item["bindings"]]
    assert len(bindings) == len({(row["addressId"], row["fixtureField"]) for row in bindings}) == 29
    new_ids = {
        "after-battle-call",
        "clear-unlocked-call",
        "set-completed-call",
        "victory-result",
        "victory-return",
        "completion-check",
        "route-load",
        "execute-script-call",
        "execute-script-resume",
        "battle01-row",
        "program-end",
        "entity-table",
        "join-call",
        "return",
        "battle-loop-call",
        "post-battle-resume",
        "exploration-call",
    }
    addresses = [
        address
        for record, _ in evidence
        for address in record["addresses"]
        if address["id"] in new_ids
    ]
    assert len(addresses) == 18
    assert {row["value"] for row in addresses} >= {0x23D08, 0x497F4, 0x497F6, 0x75E4}


def test_index_contract_rejects_binding_address_document_and_accepted_base_drift() -> None:
    index = json.loads((ROOT / "manifests/research-index.json").read_text(encoding="utf-8"))
    victory_return._owner_evidence(index)

    malformed = copy.deepcopy(index)
    record = next(item for item in malformed["records"] if item["id"] == "battle.control.outcomes")
    record["evidence"][-1]["bindings"][0]["fixtureField"] = "victoryReturnSpine.wrong"
    with pytest.raises(ValueError, match="owner evidence drift"):
        victory_return._owner_evidence(malformed)

    malformed = copy.deepcopy(index)
    record = next(item for item in malformed["records"] if item["id"] == "battle.control.outcomes")
    record["addresses"][-1]["value"] += 2
    with pytest.raises(ValueError, match="address delta drift"):
        victory_return._owner_evidence(malformed)

    malformed = copy.deepcopy(index)
    record = next(item for item in malformed["records"] if item["id"] == "battle.control.outcomes")
    record["documents"][-1] = "docs/research/wrong.md"
    with pytest.raises(ValueError, match="owner document drift"):
        victory_return._owner_evidence(malformed)

    malformed = copy.deepcopy(index)
    record = next(item for item in malformed["records"] if item["id"] == "battle.control.outcomes")
    record["designContracts"].append("docs/design/contracts/wrong.md")
    with pytest.raises(ValueError, match="accepted-base index drift"):
        victory_return._owner_evidence(malformed)


@pytest.mark.parametrize(
    "mutation,error",
    (
        (
            lambda record: record.__setitem__("sourcePath", "code/wrong.asm"),
            "owner source drift",
        ),
        (
            lambda record: record["addresses"].__setitem__(
                -1, {**record["addresses"][-1], "kind": "symbol"}
            ),
            "address delta drift",
        ),
        (
            lambda record: record["evidence"].__setitem__(
                -1,
                {**record["evidence"][-1], "verifier": "src/sf2tool/h2/wrong.py"},
            ),
            "owner evidence drift",
        ),
        (
            lambda record: record["evidence"][-1]["bindings"].reverse(),
            "owner evidence drift",
        ),
    ),
    ids=("source", "address-kind", "verifier", "binding-order"),
)
def test_index_contract_rejects_every_authorized_delta_dimension(
    mutation: object, error: str
) -> None:
    malformed = json.loads((ROOT / "manifests/research-index.json").read_text(encoding="utf-8"))
    record = next(item for item in malformed["records"] if item["id"] == "battle.control.outcomes")
    mutation(record)  # type: ignore[operator]
    with pytest.raises(ValueError, match=error):
        victory_return._owner_evidence(malformed)


def test_index_contract_rejects_missing_extra_and_unrelated_owner_record_drift() -> None:
    index = json.loads((ROOT / "manifests/research-index.json").read_text(encoding="utf-8"))

    malformed = copy.deepcopy(index)
    record = next(item for item in malformed["records"] if item["id"] == "battle.control.outcomes")
    record["evidence"].pop()
    with pytest.raises(ValueError, match="owner evidence drift"):
        victory_return._owner_evidence(malformed)

    malformed = copy.deepcopy(index)
    record = next(item for item in malformed["records"] if item["id"] == "battle.control.outcomes")
    record["documents"].append("docs/research/map3-battle01-victory-return.md")
    with pytest.raises(ValueError, match="owner document drift"):
        victory_return._owner_evidence(malformed)

    malformed = copy.deepcopy(index)
    record = next(item for item in malformed["records"] if item["id"] == "battle.control.outcomes")
    record["addresses"][0]["value"] += 2
    with pytest.raises(ValueError, match="entry address drift"):
        victory_return._owner_evidence(malformed)

    malformed = copy.deepcopy(index)
    extra = next(
        item for item in malformed["records"] if item["id"] == "battle.loop.heal-living-allies"
    )
    extra["evidence"].append(
        {
            "level": "H2",
            "fixture": "tests/fixtures/h2/map3-battle01-victory-return-static-v1.json",
            "fixtureId": victory_return.ID,
            "verifier": "src/sf2tool/h2/map3_battle01_victory_return.py",
            "bindings": [],
        }
    )
    with pytest.raises(ValueError, match="owner evidence drift"):
        victory_return._owner_evidence(malformed)


def test_summary_is_derived_from_the_closed_denominators() -> None:
    fixture = json.loads(victory_return.FIXTURE.read_text(encoding="utf-8"))
    summary = victory_return._summary(
        fixture["sourceContext"]["sourceIdentities"],
        fixture["sourceContext"]["h1RomAnchors"],
        victory_return._owner_evidence(
            json.loads((ROOT / "manifests/research-index.json").read_text(encoding="utf-8"))
        ),
        fixture["unknowns"],
        fixture["victoryReturnSpine"]["battle01AfterBattleProgram"],
    )
    assert summary == fixture["summary"]
