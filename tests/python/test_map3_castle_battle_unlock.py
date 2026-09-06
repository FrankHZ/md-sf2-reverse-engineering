"""Static-only contract tests for the Map 3 castle-to-Battle 01 fallback rail."""

from __future__ import annotations

import re
import shutil
from copy import deepcopy
from pathlib import Path
from types import SimpleNamespace

import pytest

from sf2tool.h2 import map3_castle_battle_unlock as rail
from sf2tool.jsonio import load_json, validate_json


def _fixture() -> dict[str, object]:
    return load_json(rail.FIXTURE)


def _zone_sources() -> dict[str, str]:
    sources: dict[str, str] = {}
    for map_id, _, symbol, _, _, _, expected in rail._ZONE_SPECS:
        lines = [f"{symbol}:"]
        for kind, point, target, _ in expected:
            if kind == "default":
                lines.append(f"msDefaultZoneEvent {target}-")
            else:
                lines.append(f"msZoneEvent {point[0]},{point[1]},{target}-")
        sources[f"data/maps/entries/map{map_id:02d}/mapsetups/s3_zoneevents.asm"] = "\n".join(lines)
    return sources


def test_fixture_is_closed_public_static_contract() -> None:
    fixture = _fixture()
    validate_json(fixture, rail.SCHEMA, owner="Map 3 castle static fixture")
    assert fixture["id"] == rail.ID
    assert rail.canonical_json_bytes(fixture) == rail.FIXTURE.read_bytes()
    assert fixture["summary"] == {
        "functions": 23,
        "h1Fields": 32,
        "logicalInputs": 110,
        "programs": 6,
        "routeSegments": 16,
        "sourceFiles": 53,
        "zoneEncodedBytes": 52,
        "zoneRows": 13,
        "zoneTables": 4,
    }
    function_addresses = fixture["sourceContext"]["functionAddresses"]  # type: ignore[index]
    programs = fixture["static"]["programs"]  # type: ignore[index]
    for symbol in ("cs_51652", "cs_53104", "cs_53996", "cs_52F0C", "cs_52F40", "cs_53EF4"):
        assert programs[symbol]["address"] == function_addresses[symbol]
        assert re.fullmatch(r"[0-9A-F]{64}", programs[symbol]["controlEffectSha256"])
    assert programs["cs_51652"]["semantics"] == {"callerSetFlags": [604], "role": "castle-gate"}


def test_fixture_rejects_missing_and_extra_public_fields() -> None:
    fixture = _fixture()
    missing = deepcopy(fixture)
    del missing["static"]["routeGraph"]  # type: ignore[index]
    with pytest.raises(ValueError, match="validation"):
        validate_json(missing, rail.SCHEMA, owner="missing route graph")
    extra = deepcopy(fixture)
    extra["static"]["runtimeObservation"] = {}  # type: ignore[index]
    with pytest.raises(ValueError, match="validation"):
        validate_json(extra, rail.SCHEMA, owner="runtime boundary")
    reordered = deepcopy(fixture)
    reordered["static"]["routeGraph"]["segments"] = list(  # type: ignore[index]
        reversed(reordered["static"]["routeGraph"]["segments"])  # type: ignore[index]
    )
    with pytest.raises(ValueError, match="validation"):
        validate_json(reordered, rail.SCHEMA, owner="route order")
    changed = deepcopy(fixture)
    changed["static"]["routeGraphSha256"] = "0" * 64  # type: ignore[index]
    with pytest.raises(ValueError, match="validation"):
        validate_json(changed, rail.SCHEMA, owner="route digest")


def test_royal_return_graph_uses_the_retained_warp_facing() -> None:
    static = _fixture()["static"]
    warp = static["warps"]["map20Royal"][0]
    segment = next(
        row
        for row in static["routeGraph"]["segments"]
        if row["id"] == "map20-to-map19-royal-return"
    )
    assert segment["to"]["facing"] == warp["facing"]


@pytest.fixture
def royal_warp_inputs(tmp_path: Path) -> tuple:
    """Project-authored rows isolate the fifth record without private payloads."""
    rows = []
    for index in range(11):
        x, y, dx, dy = (23, 37, 23, 3) if index == 4 else (index, 10, index, 1)
        rows.append(
            f"mWarp {x}, {y}\nwarpNoScroll\nwarpMap MAP_GRANSEAL_CASTLE_2F\n"
            f"warpDest {dx}, {dy}\nwarpFacing LEFT\n"
        )
    source = "\n".join(rows) + "endWord\n"
    constants = {"LEFT": 2, "DOWN": 3, "MAP_GRANSEAL_CASTLE_2F": 19}
    path = tmp_path / "synthetic-warps.asm"
    path.write_text(source, encoding="utf-8")
    encoded, count, trailing = rail._encode_source(path, "warpEvents", constants)
    assert (count, trailing) == (11, False)
    address = 0xA53DA
    rom = bytes(address) + encoded
    return source, constants, encoded, {"Map20s6_WarpEvents": address}, rom, rom


def test_royal_warp_derives_exact_record_and_ignores_comments(royal_warp_inputs: tuple) -> None:
    source, *rest = royal_warp_inputs
    expected = {"from": [23, 37], "to": [23, 3], "facing": "LEFT"}
    assert rail._royal_return_warp(source, *rest) == expected
    commented = "; mWarp 23,37; warpFacing DOWN\n" + source.replace(
        "warpFacing LEFT", "\twarpFacing   LEFT ; warpFacing DOWN near-miss"
    )
    assert rail._royal_return_warp(commented, *rest) == expected


@pytest.mark.parametrize(
    ("old", "new"),
    [
        ("mWarp 23, 37", "mWarp 24, 37"),
        ("warpDest 23, 3", "warpDest 23, 4"),
        ("warpDest 23, 3", "warpDest 23, 259"),
        ("warpDest 23, 3", "warpDest 23, 3, 0"),
        ("warpDest 23, 3", "warpDestExtra 23, 3"),
        ("warpDest 23, 3", "near_miss: warpDest 23, 3"),
        ("warpNoScroll", "warpNoScroll 0"),
        ("warpNoScroll", "warpScroll DOWN"),
        ("warpMap MAP_GRANSEAL_CASTLE_2F", "warpMap 20"),
        ("warpMap MAP_GRANSEAL_CASTLE_2F", "warpMap 19"),
        ("warpDest 23, 3\nwarpFacing LEFT", "warpFacing LEFT\nwarpDest 23, 3"),
        ("warpDest 23, 3\nwarpFacing LEFT", "warpDest 23, 3\nwarpFacing DOWN"),
        ("endWord", "endWord\nwarpFacing LEFT"),
    ],
)
def test_royal_warp_rejects_source_operand_order_and_identity_drift(
    royal_warp_inputs: tuple, old: str, new: str
) -> None:
    source, *rest = royal_warp_inputs
    assert old in source
    with pytest.raises(ValueError):
        rail._royal_return_warp(source.replace(old, new), *rest)


def test_royal_warp_rejects_reordered_records_and_enum_drift(royal_warp_inputs: tuple) -> None:
    source, constants, encoded, addresses, h1, rom = royal_warp_inputs
    rows = source.split("\n\n")
    rows[3], rows[4] = rows[4], rows[3]
    with pytest.raises(ValueError, match="field use-site drift"):
        rail._royal_return_warp("\n\n".join(rows), constants, encoded, addresses, h1, rom)
    with pytest.raises(ValueError, match="field use-site drift"):
        rail._royal_return_warp(source, {**constants, "LEFT": 3}, encoded, addresses, h1, rom)


@pytest.mark.parametrize("input_index", [2, 4, 5])
def test_royal_warp_requires_source_h1_rom_agreement(
    royal_warp_inputs: tuple, input_index: int
) -> None:
    args = list(royal_warp_inputs)
    changed = bytearray(args[input_index])
    offset = (0 if input_index == 2 else 0xA53DA) + 4 * 8 + 6
    changed[offset] = 3
    args[input_index] = bytes(changed)
    with pytest.raises(ValueError, match="source/H1/ROM record drift"):
        rail._royal_return_warp(*args)


def test_royal_warp_rejects_h1_identity_and_decoder_drift(
    royal_warp_inputs: tuple, monkeypatch: pytest.MonkeyPatch
) -> None:
    source, constants, encoded, addresses, h1, rom = royal_warp_inputs
    with pytest.raises(ValueError, match="H1 table identity/width drift"):
        rail._royal_return_warp(source, constants, encoded, {}, h1, rom)
    with pytest.raises(ValueError, match="source/H1/ROM record drift"):
        rail._royal_return_warp(source, constants, encoded, addresses, h1[:-1], rom)
    decoder = rail._decode_warps

    def wrong_facing(data: bytes, count: int) -> list:
        rows = decoder(data, count)
        rows[4]["facing"] = 3
        return rows

    monkeypatch.setattr(rail, "_decode_warps", wrong_facing)
    with pytest.raises(ValueError, match="field use-site drift"):
        rail._royal_return_warp(*royal_warp_inputs)


@pytest.mark.parametrize("endpoint", ["from", "to"])
def test_royal_warp_graph_consumer_rejects_facing_drift_before_golden(endpoint: str) -> None:
    static = _fixture()["static"]
    graph = deepcopy(static["routeGraph"])
    warps = [join["row"] for join in static["retainedWarpJoins"]]
    segment = next(row for row in graph["segments"] if row["id"] == "map20-to-map19-royal-return")
    segment[endpoint]["facing"] = "DOWN"
    with pytest.raises(ValueError, match="royal warp graph use-site drift"):
        rail._retained_warp_joins(graph, warps, static["warps"])


def test_retained_warp_predicate_requires_exact_or_wildcard_single_match() -> None:
    row = {
        "fromMap": 3,
        "toMap": 19,
        "eventDestinationMap": 19,
        "x": 255,
        "y": 1,
        "destinationX": 26,
        "destinationY": 30,
    }
    assert (
        rail._retained_warp_predicate(
            [row],
            segment="map3-to-map19-north-warp",
            source_map=3,
            source_point=(28, 1),
            destination_map=19,
            destination_point=(26, 30),
        )
        == row
    )
    with pytest.raises(ValueError, match="multiplicity"):
        rail._retained_warp_predicate(
            [row, dict(row)],
            segment="map3-to-map19-north-warp",
            source_map=3,
            source_point=(28, 1),
            destination_map=19,
            destination_point=(26, 30),
        )
    with pytest.raises(ValueError, match="destination"):
        rail._retained_warp_predicate(
            [{**row, "destinationY": 29}],
            segment="map3-to-map19-north-warp",
            source_map=3,
            source_point=(28, 1),
            destination_map=19,
            destination_point=(26, 30),
        )


def test_program_control_effect_digest_distinguishes_order_and_omits_prose() -> None:
    source = """cs_test:  entityActions 128
 moveRight 1
endActions ; private source prose must not enter the digest
csc_end
"""
    reordered = """cs_test:  entityActions 128
endActions
moveRight 1
csc_end
"""
    assert rail._program_control_effect_sha256(source) != rail._program_control_effect_sha256(
        reordered
    )
    assert rail._program_control_effect_sha256(source) == rail._program_control_effect_sha256(
        source.replace("private source prose must not enter the digest", "different prose")
    )
    with pytest.raises(ValueError, match="source-use drift"):
        rail._require_order(
            reordered,
            ("entityActions 128", "moveRight 1", "endActions"),
            "program order",
        )
    with pytest.raises(ValueError, match="source-use drift"):
        rail._require_order(
            source.replace("moveRight 1", "moveRight 2"),
            ("entityActions 128", "moveRight 1", "endActions"),
            "program semantic",
        )


def test_map3_zone_event4_owns_f604_not_cs_51652() -> None:
    zone_source = """Map3_ZoneEvent4:
 chkFlg  604
 script  cs_51652
 setFlg  604
 rts
"""
    program_source = """cs_51652:
 entityActions 138
 moveRight 1
 endActions
 csc_end
"""
    rail._map3_castle_gate_flag_owner(zone_source, program_source)
    with pytest.raises(ValueError, match="source-use drift"):
        rail._map3_castle_gate_flag_owner(
            zone_source.replace("script  cs_51652\n setFlg  604", "setFlg  604\n script  cs_51652"),
            program_source,
        )
    with pytest.raises(ValueError, match="must not own F604"):
        rail._map3_castle_gate_flag_owner(
            zone_source,
            program_source.replace(" csc_end", " setF 604\n csc_end"),
        )


def test_build_rejects_canonical_rom_hash_and_upstream_revision(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    rom = tmp_path / "input.bin"
    rom.write_bytes(bytes(0x200))
    with pytest.raises(ValueError, match="canonical ROM SHA-256 drift"):
        rail.build_map3_castle_battle_unlock_static(rom, tmp_path / "not-reached")

    upstream = tmp_path / "upstream"
    upstream.mkdir()
    monkeypatch.setattr(rail, "inspect_rom", lambda _: {"sha256": rail._ROM_SHA256})
    monkeypatch.setattr(
        rail.subprocess,
        "run",
        lambda *args, **kwargs: SimpleNamespace(stdout="not-the-pinned-revision\n"),
    )
    with pytest.raises(ValueError, match="upstream revision drift"):
        rail.build_map3_castle_battle_unlock_static(rom, upstream)


def test_h1_zone_collision_and_guard_mutations_fail_closed() -> None:
    span_end = max(
        address + width
        for _, address, width in [
            *((name, address, 2) for name, address in rail._FUNCTIONS.items()),
            rail._MAP19_TABLE,
            *rail._INPUT_H1,
        ]
    )
    h1 = bytes(span_end)
    assert len(rail._h1_projection(h1, h1)) == 32
    mismatched_rom = bytearray(h1)
    mismatched_rom[rail._FUNCTIONS["ApplyZ80BusUpdates"]] = 1
    with pytest.raises(ValueError, match="H1/ROM drift"):
        rail._h1_projection(h1, bytes(mismatched_rom))
    with pytest.raises(ValueError, match="H1 binary span is incomplete"):
        rail._h1_projection(b"", b"")

    zone_end = max(
        address + 4 * len(expected) for _, _, _, address, _, _, expected in rail._ZONE_SPECS
    )
    zones = _zone_sources()
    assert rail._zones(zones, bytes(zone_end), bytes(zone_end))["denominator"] == {
        "tableCount": 4,
        "recordCount": 13,
        "encodedBytes": 52,
    }
    map3 = "data/maps/entries/map03/mapsetups/s3_zoneevents.asm"
    missing_row = dict(zones)
    missing_row[map3] = "\n".join(zones[map3].splitlines()[:-1])
    with pytest.raises(ValueError, match="zone row denominator drift"):
        rail._zones(missing_row, bytes(zone_end), bytes(zone_end))
    reordered_rows = dict(zones)
    lines = reordered_rows[map3].splitlines()
    lines[1], lines[2] = lines[2], lines[1]
    reordered_rows[map3] = "\n".join(lines)
    with pytest.raises(ValueError, match="zone source row drift"):
        rail._zones(reordered_rows, bytes(zone_end), bytes(zone_end))

    controller = {
        "collisionMask": 0xC000,
        "rightStairMask": 0x8000,
        "leftStairMask": 0x4000,
        "stairWordDeltas": [-63, 63, 65, -65],
    }
    surface = {"width": 2, "layout": [0, 0xC000, 0, 0], "areas": ((0, 0, 1, 1),)}
    assert rail._move(surface, (0, 0), "Right", controller) is None
    surface["layout"][1] = 0
    assert rail._move(surface, (0, 0), "Right", controller) == (1, 0)

    guards = """cs_5149A:
 setPos 138,27,3,UP
 setPos 139,31,3,UP
cs_51652:
 entityActions 138
 moveRight 1
 endActions
 entityActionsWait 139
 moveLeft 1
 endActions
 entityActions 138
 moveLeft 1
 endActions
 entityActionsWait 139
 moveRight 1
 endActions
 csc_end
"""
    assert rail._map3_restored_guards(guards) == frozenset({(27, 3), (31, 3)})
    with pytest.raises(ValueError, match="guard move order drift"):
        rail._map3_restored_guards(guards.replace("moveRight 1", "moveRight 2", 1))


def test_route_prefix_and_output_mutations_reject_before_fixture_acceptance(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = _fixture()
    graph = deepcopy(fixture["static"]["routeGraph"])  # type: ignore[index]
    assert rail._validate_route_graph(graph) == fixture["static"]["routeGraphSha256"]  # type: ignore[index]
    missing_segment = deepcopy(graph)
    missing_segment["segments"].pop()  # type: ignore[index]
    with pytest.raises(ValueError, match="denominator/order drift"):
        rail._validate_route_graph(missing_segment)
    missing_input = deepcopy(graph)
    missing_input["segments"][0]["inputs"].pop()  # type: ignore[index]
    with pytest.raises(ValueError, match="denominator/order drift"):
        rail._validate_route_graph(missing_input)
    changed_graph = deepcopy(graph)
    changed_graph["schoolDoor"]["source"][0] = 61  # type: ignore[index]
    with pytest.raises(ValueError, match="graph digest drift"):
        rail._validate_route_graph(changed_graph)

    prefix = deepcopy(fixture)
    prefix["retainedPrefixGuards"]["acceptedPrefixFixtures"][0]["sha256"] = "0" * 64  # type: ignore[index]
    with pytest.raises(ValueError, match="structural schema validation failed"):
        rail._validate_structural_output(prefix)
    monkeypatch.setattr(rail, "build_map3_castle_battle_unlock_static", lambda *_: prefix)
    with pytest.raises(ValueError, match="complete semantic fixture drift"):
        rail.verify_map3_castle_battle_unlock_static(Path("unused-rom"), Path("unused-upstream"))
    bad_prefix = tmp_path / "bad-prefix.json"
    bad_prefix.write_text('{"id":"wrong"}', encoding="utf-8")
    monkeypatch.setattr(rail, "repo_path", lambda _: bad_prefix)
    with pytest.raises(ValueError, match="retained prefix identity drift"):
        rail._retained_prefix()


def test_static_fixture_has_no_runtime_artifact_boundary() -> None:
    payload = rail.FIXTURE.read_text(encoding="utf-8").lower()
    for forbidden in (
        "observer",
        "callback",
        "bootstrap",
        "cleanup",
        "emulator",
        "movie",
        "savestate",
        "runtimegolden",
        "inputdriver",
    ):
        assert forbidden not in payload


def test_build_matches_fixture_when_normal_private_inputs_are_available() -> None:
    rom = rail.repo_path("local/roms/sf2-us.bin")
    upstream = rail.repo_path("local/upstream/SF2DISASM")
    if not rom.is_file() or not upstream.is_dir():
        pytest.skip("normal private ROM/upstream inputs are unavailable")
    assert rail.verify_map3_castle_battle_unlock_static(rom, upstream) == _fixture()


def test_mutated_normal_rom_rejects_when_private_inputs_are_available(tmp_path: Path) -> None:
    rom = rail.repo_path("local/roms/sf2-us.bin")
    upstream = rail.repo_path("local/upstream/SF2DISASM")
    if not rom.is_file() or not upstream.is_dir():
        pytest.skip("normal private ROM/upstream inputs are unavailable")
    mutated = tmp_path / "mutated-sf2-us.bin"
    shutil.copyfile(rom, mutated)
    with mutated.open("r+b") as output:
        original = output.read(1)
        output.seek(0)
        output.write(bytes([original[0] ^ 0xFF]))
    with pytest.raises(ValueError, match="canonical ROM SHA-256 drift"):
        rail.build_map3_castle_battle_unlock_static(mutated, upstream)
