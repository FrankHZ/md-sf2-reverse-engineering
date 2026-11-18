from __future__ import annotations

import json
import re
from argparse import ArgumentParser
from pathlib import Path

import pytest

from sf2tool import research_index
from sf2tool.cli import build_parser, full_verify_requested
from sf2tool.compression import decode_basic_compressed, decode_stack_compressed
from sf2tool.design_contracts import verify_design_contracts
from sf2tool.h2.battle_ai import _direct_target, _parse_source_file
from sf2tool.h2.map_content import _encode_source
from sf2tool.h2.map_descriptions import _decode_entry
from sf2tool.h2.map_entities import _record_kind
from sf2tool.h2.map_events import (
    _clean_state_event_indices,
    _decode_event_record,
    _event_matches,
)
from sf2tool.h2.map_layouts import decode_map_blocks
from sf2tool.h2.map_setup import _parse_routes, _select_route
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.research_index import listing_symbol_addresses, verify_index
from sf2tool.rom import mega_drive_checksum


def test_design_contracts_are_traceable() -> None:
    assert verify_design_contracts() == {
        "Documents": 68,
        "FixtureReferences": 204,
        "EvidenceLabels": "Confirmed,Unknown",
        "Status": "PASS",
    }


def test_research_index_validates_without_private_inputs() -> None:
    result = verify_index()
    assert result["Status"] == "PASS"
    assert result["Records"] == 1627
    assert result["Confirmed"] == 1627
    assert result["H2Fixtures"] == 103
    assert result["H3Fixtures"] == result["H3FixtureFiles"] == 96
    assert result["AddressBindings"] == 3111
    assert result["IndexedCodeFiles"] == 381
    assert result["IndexedDataFiles"] == 1017
    assert result["H1ListingRecords"] == 1590
    assert result["AlternateListingRecords"] == 37
    assert result["Z80MusicBankRecords"] == 37
    assert result["ResearchDocuments"] == 66
    assert result["DesignContracts"] == 68


def test_research_index_rejects_duplicate_evidence_address_ids(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = json.loads(research_index.INDEX_PATH.read_text(encoding="utf-8"))
    headshake = next(
        record
        for record in index["records"]
        if record["id"] == "map.entity-presentation-fx.headshake"
    )
    return_binding = next(
        binding
        for binding in headshake["evidence"][0]["bindings"]
        if binding["fixtureField"] == "function.csc27ReturnAddress"
    )
    return_binding["addressId"] = "final-anim-after"
    mutated_index = tmp_path / "research-index-duplicate-binding.json"
    mutated_index.write_bytes((json.dumps(index, indent=2) + chr(10)).encode("utf-8"))
    monkeypatch.setattr(research_index, "INDEX_PATH", mutated_index)
    with pytest.raises(ValueError, match="duplicate evidence address ID"):
        research_index.verify_index()


def test_listing_symbol_addresses_indexes_once_and_rejects_conflicts() -> None:
    listing = "00000010 4E75 First:\n00000020 Second:\n00000010 First:\n"
    assert listing_symbol_addresses(listing) == {"First": 0x10, "Second": 0x20}

    with pytest.raises(ValueError, match="conflicting addresses"):
        listing_symbol_addresses("00000010 First:\n00000012 First:\n")


def test_mega_drive_checksum_handles_an_odd_trailing_byte() -> None:
    data = bytearray(0x203)
    data[0x200:] = b"\x12\x34\x56"
    assert mega_drive_checksum(bytes(data)) == "6834"


@pytest.fixture(scope="module")
def cli_parser() -> ArgumentParser:
    return build_parser()


@pytest.mark.parametrize("command", [
    "enemy-gold",
    "enemy-drops",
    "map-script-engine",
    "remaining-core",
])
def test_static_default_output(cli_parser: ArgumentParser, command: str) -> None:
    args = cli_parser.parse_args(["h2", command])
    assert args.h2_command == command
    assert args.output_path is None


@pytest.mark.parametrize("command", [
    "map3-castle-battle-unlock",
    "map3-battle01-admission",
    "map3-battle01-turn-control",
    "map3-battle01-action-effect",
    "map3-battle01-action-completion",
    "map3-battle01-turn-finalization",
    "map3-battle01-victory-return",
    "map3-original-player-reference-frame",
])
def test_static_input_paths(cli_parser: ArgumentParser, command: str) -> None:
    args = cli_parser.parse_args(["h2", command])
    assert args.h2_command == command
    assert args.rom_path
    assert args.upstream_path


@pytest.mark.parametrize("command", [
    "battle-ai",
    "battle-scene-engine",
    "battle-scene-animations",
    "battle-cutscenes",
    "common-scripting",
    "common-maps",
    "common-stats",
    "common-menus",
    "tech-interrupts",
    "tech-graphics",
    "tech-interfaces",
    "tech-services",
    "gameflow-core",
    "special-screens",
    "battle-global-data",
    "ally-data",
    "core-stats-data",
    "battle-cutscene-data",
    "battle-spriteset-data",
    "battle-routing-data",
    "compression-consumers",
    "map-data",
    "map-scripts",
    "auxiliary-data",
])
def test_source_only_output(cli_parser: ArgumentParser, command: str) -> None:
    args = cli_parser.parse_args(["h2", command])
    assert args.h2_command == command
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


@pytest.mark.parametrize("command", [
    "field-menu-control",
    "field-item-effects",
    "field-search-control",
])
def test_rom_and_upstream_defaults(cli_parser: ArgumentParser, command: str) -> None:
    args = cli_parser.parse_args(["h2", command])
    assert args.h2_command == command
    assert args.rom_path.name == "sf2-us.bin"
    assert args.upstream_path.name == "SF2DISASM"
    assert not hasattr(args, "output_path")


@pytest.mark.parametrize("command", [
    "item-auxiliary",
    "enemy-map-sprites",
    "map-sprite-assignments",
    "entity-action-scripts",
    "sprite-dialogue",
    "battle-terrain",
    "battle-backgrounds",
    "battle-sprites",
    "battle-sprite-animations",
    "battle-weapon-ground",
    "portraits",
    "map-sprites",
    "special-sprites",
    "special-screen-graphics",
    "special-screen-presentation",
    "ui-graphics",
    "ui-layouts",
    "variable-width-font",
    "text-huffman",
    "text-banks",
    "unused-tech-assets",
    "witch-menu-graphics",
    "icon-graphics",
    "battle-effect-graphics",
    "map-tilesets",
    "map-palettes",
    "map-setup",
    "map-entities",
    "map-content",
    "map-events",
    "map-event-direct-state",
    "map-event-flag-lifecycle-state",
    "map-event-cross-program-flag-state",
    "map-event-flag-route-selection",
    "map-event-interaction-state",
    "map-event-item-transactions",
    "map-event-combatant-state",
    "map-event-random-battle-state",
    "map-event-tactical-base-quote-state",
    "map-event-scripted-transition-state",
    "map-event-request-state",
    "map-event-request-consumption",
    "map-event-direct-handoff",
    "map-event-predicate-results",
    "map-event-dialogue-state",
    "map-descriptions",
    "map-init",
    "sound-data",
])
def test_rom_output_defaults(cli_parser: ArgumentParser, command: str) -> None:
    args = cli_parser.parse_args(["h2", command])
    assert args.h2_command == command
    assert args.output_path is None
    assert args.rom_path.name == "sf2-us.bin"


@pytest.mark.parametrize("command", [
    "map-layouts",
])
def test_layout_rom_default(cli_parser: ArgumentParser, command: str) -> None:
    args = cli_parser.parse_args(["h2", command])
    assert args.h2_command == command
    assert args.rom_path.name == "sf2-us.bin"


@pytest.mark.parametrize(("command", "timeout"), [
    ("map-event-dispatch", 120),
    ("map-lifecycle", 120),
    ("map-interaction-trigger", 120),
    ("map-camera-control", 120),
    ("map-entity-placement", 120),
    ("witch-save-actions", 120),
    ("witch-new-game-lifecycle", 120),
    ("map-animation-vdp", 120),
    ("sound-timing", 120),
    ("entity-movement", 120),
    ("map-script-dialogue", 180),
    ("story-state", 180),
    ("map-script-entity-clone", 180),
    ("witch-save-menu-actions", 180),
    ("map3-admitted-start", 180),
    ("map3-original-player-locomotion-animation", 240),
    ("map3-battle01-natural-route", 300),
    ("map3-messenger-acceptance", 300),
    ("map3-battle01-player-ready", 600),
])
def test_runtime_rom_and_timeout(cli_parser: ArgumentParser, command: str, timeout: int) -> None:
    args = cli_parser.parse_args(["h3", command])
    assert args.h3_command == command
    assert args.timeout_seconds == timeout
    assert args.rom_path.name == "sf2-us.bin"


@pytest.mark.parametrize(("command", "timeout"), [
    ("growth-refresh", 60),
    ("gold", 60),
    ("kill-exp", 75),
    ("exp-command", 75),
    ("enemy-drops", 75),
    ("award-exp", 90),
    ("muddle-action-guard", 120),
])
def test_runtime_timeout(cli_parser: ArgumentParser, command: str, timeout: int) -> None:
    args = cli_parser.parse_args(["h3", command])
    assert args.h3_command == command
    assert args.timeout_seconds == timeout


def test_verify_defaults_to_commit_profile_and_full_is_explicit() -> None:
    commit_args = build_parser().parse_args(["verify"])
    assert commit_args.command == "verify"
    assert commit_args.full is False

    full_args = build_parser().parse_args(["verify", "--full"])
    assert full_args.full is True


def test_verify_quick_remains_a_hidden_compatibility_alias() -> None:
    args = build_parser().parse_args(["verify", "--quick"])
    assert args.quick is True
    assert args.full is False
    assert full_verify_requested(args) is False


def test_legacy_skip_flags_still_select_the_full_profile() -> None:
    args = build_parser().parse_args(["verify", "--skip-runtime"])
    assert full_verify_requested(args) is True

    quick_args = build_parser().parse_args(["verify", "--quick", "--skip-runtime"])
    assert full_verify_requested(quick_args) is False


def test_original_reference_replay_capability_defaults_to_explicit_preflight() -> None:
    args = build_parser().parse_args(
        ["h3", "original-reference-replay-capability", "--preflight-only"]
    )
    assert args.h3_command == "original-reference-replay-capability"
    assert args.preflight_only is True
    assert args.run_class is None
    assert args.launch_ordinal is None
    assert args.timeout_seconds == 120


def test_original_reference_scenario_api_requires_its_public_preflight() -> None:
    args = build_parser().parse_args(
        ["h3", "original-reference-replay-scenario-api", "--preflight-only"]
    )

    assert args.h3_command == "original-reference-replay-scenario-api"
    assert args.preflight_only is True
    assert not hasattr(args, "rom_path")
    assert not hasattr(args, "timeout_seconds")


def test_stack_decoder_handles_literal_overlap_copy_and_terminator() -> None:
    def packed(bits: str) -> bytes:
        bits += "0" * (-len(bits) % 8)
        return int(bits, 2).to_bytes(len(bits) // 8, "big")

    # Custom command nibble 6 followed by three zero nibbles gives command word 0x6000:
    # literal zero, offset-one copy of two words, then the zero-offset terminator.
    stream = packed("11110110" + "000" + "00" * 4 + f"{1:011b}" + "1" + "0" * 11)
    result = decode_stack_compressed(stream, expected_output_bytes=6)
    assert result.output == bytes(6)
    assert result.literal_word_count == 1
    assert result.copy_command_count == 1
    assert result.copied_word_count == 2
    assert result.maximum_copy_offset_words == 1
    assert result.maximum_copy_length_words == 2


def test_basic_decoder_handles_literal_overlap_copy_and_terminator() -> None:
    # Bitmap 0,1,1: literal 0x1234, offset-one/length-two copy, terminator.
    stream = bytes.fromhex("6000 1234 003F 0000")
    result = decode_basic_compressed(stream, expected_output_bytes=6)
    assert result.output == bytes.fromhex("1234 1234 1234")
    assert result.command_word_count == 1
    assert result.literal_word_count == 1
    assert result.copy_command_count == 1
    assert result.copied_word_count == 2
    assert result.repeat_last_word_command_count == 1
    assert result.maximum_copy_offset_words == 1
    assert result.maximum_copy_length_words == 2


def test_map_setup_selection_uses_the_last_set_flag_in_source_order() -> None:
    routes = _parse_routes(
        "MapSetups: msMap 3, ms_map3\n"
        " msFlag 10, ms_map3_flag10\n"
        " msFlag 20, ms_map3_flag20\n"
        " msMapEnd\n"
        " msEnd\n"
    )
    assert _select_route(routes, 3, set()) == "ms_map3"
    assert _select_route(routes, 3, {10, 20}) == "ms_map3_flag20"
    assert _select_route(routes, 4, {10, 20}) == "ms_Void"


def test_map_content_encoder_keeps_item_terminator_and_trailing_rts(tmp_path: Path) -> None:
    source = tmp_path / "8-other-items.asm"
    source.write_text(
        "mapItem 1, 2, 3, HEALING_SEED\nendWord\nrts\n",
        encoding="utf-8",
    )
    encoded, records, trailing_rts = _encode_source(
        source,
        "otherItems",
        {"ITEM_HEALING_SEED": 1},
    )
    assert encoded == bytes.fromhex("01020301FFFF4E75")
    assert records == 1
    assert trailing_rts is True


def test_zero_command_block_stream_yields_the_three_builtin_blocks() -> None:
    words, consumed_bits, commands = decode_map_blocks(b"\x00\x00")
    assert len(words) == 27
    assert consumed_bits == 14
    assert commands == {}


def test_map_entity_payload_prefix_classifies_record_encoding() -> None:
    assert _record_kind(bytes.fromhex("0102030400001234")) == "fixed"
    assert _record_kind(bytes.fromhex("01020304FF050607")) == "walking"
    assert _record_kind(bytes.fromhex("01020304FE001234")) == "sequenced"


def test_map_entity_event_indices_split_allies_from_stream_order_enemies() -> None:
    records = [
        {"mapSprite": 2},
        {"mapSprite": 122},
        {"mapSprite": 5},
        {"mapSprite": 132},
    ]
    assert _clean_state_event_indices(records) == [2, 128, 5, 129]


def test_map_event_matcher_handles_wildcards_defaults_and_item_mask() -> None:
    assert _event_matches("zoneEvents", {"kind": "specific", "x": 0xFF, "y": 12}, {"x": 7, "y": 12})
    assert _event_matches(
        "itemEvents",
        {"kind": "specific", "x": 1, "y": 2, "facing": 0xFF, "item": 112},
        {"x": 1, "y": 2, "facing": 3, "item": 240},
    )
    assert _event_matches("entityEvents", {"kind": "default"}, {"entity": 254})


def test_map_event_relative_offsets_resolve_from_table_base() -> None:
    record = _decode_event_record("zoneEvents", 0x1000, 0x1004, bytes.fromhex("FD000020"))
    assert record == {
        "address": 0x1004,
        "kind": "default",
        "relativeOffset": 0x20,
        "resolvedTargetAddress": 0x1020,
        "x": 0xFD,
        "y": 0,
    }


def test_map_description_text_indices_use_global_and_wrapper_bases() -> None:
    record = _decode_entry(0x1000, 0x1006, bytes.fromhex("010200000304"), 0x200)
    assert record["kind"] == "text"
    assert record["investigationTextIndex"] == 426
    assert record["descriptionTextIndex"] == 0x204


def test_battle_ai_inventory_classifies_calls(tmp_path: Path) -> None:
    source = tmp_path / "sample.asm"
    source.write_text(
        "Entry: bsr.w DirectCall\n"
        "@Loop: jsr (OtherCall).w\n"
        "        jsr (a0)\n"
        "loc_1234:\n"
        "        rts\n",
        encoding="utf-8",
    )
    parsed = _parse_source_file(source, "code/gameflow/battle/ai/sample.asm")
    assert parsed["globalLabels"] == ["Entry", "loc_1234"]
    assert parsed["localLabelCount"] == 1
    assert parsed["statementCount"] == 4
    assert parsed["directCalls"] == [
        {"target": "DirectCall", "siteCount": 1},
        {"target": "OtherCall", "siteCount": 1},
    ]
    assert parsed["indirectCallSiteCount"] == 1
    assert _direct_target("4(a0)") is None


def test_source_inventory_accepts_legacy_single_byte_comments(tmp_path: Path) -> None:
    source = tmp_path / "legacy.asm"
    source.write_bytes(b"Entry: and 80h ; '\x80\x90'\r\n        rts\r\n")

    parsed = _parse_source_file(source, "code/common/tech/sound/legacy.asm")

    assert parsed["globalLabels"] == ["Entry"]
    assert parsed["statementCount"] == 2


def test_legacy_powershell_surface_does_not_expand() -> None:
    root = Path(__file__).resolve().parents[2]
    scripts = sorted((root / "scripts").rglob("*.ps1"))
    lines = sum(len(path.read_text(encoding="utf-8").splitlines()) for path in scripts)
    assert len(scripts) <= 36
    assert lines <= 4813


def test_schema_tree_freezes_legacy_root_and_namespaces_new_contracts() -> None:
    root = Path(__file__).resolve().parents[2]
    schema_root = root / "schemas"
    legacy_root_schemas = tuple(schema_root.glob("*.schema.json"))
    assert len(legacy_root_schemas) <= 253

    allowed_namespaces = {"core", "h2", "h3"}
    for path in schema_root.rglob("*.schema.json"):
        relative_parts = path.relative_to(schema_root).parts
        if len(relative_parts) == 1:
            continue
        assert len(relative_parts) == 2, path
        assert relative_parts[0] in allowed_namespaces, path


def test_tracked_lua_does_not_use_reserved_words_as_dot_fields() -> None:
    root = Path(__file__).resolve().parents[2]
    keywords = (
        "and|break|do|else|elseif|end|false|for|function|goto|if|in|local|nil|not|or|"
        "repeat|return|then|true|until|while"
    )
    pattern = re.compile(rf"\.\s*(?:{keywords})\b")
    failures = []
    for path in sorted((root / "tools" / "bizhawk").glob("*.lua")):
        for line_number, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
            if pattern.search(line):
                failures.append(f"{path.name}:{line_number}: {line.strip()}")
    assert not failures, "Lua reserved word used after '.':\n" + "\n".join(failures)


def test_random_services_observer_names_duplicate_pc_registration() -> None:
    root = Path(__file__).resolve().parents[2]
    observer = (root / "tools" / "bizhawk" / "random_services_observer.lua").read_text(
        encoding="utf-8"
    )
    assert "duplicate physical-PC callback" in observer


def test_sram_lifecycle_observer_names_incomplete_copy_and_residue() -> None:
    root = Path(__file__).resolve().parents[2]
    observer = (root / "tools" / "bizhawk" / "sram_lifecycle_observer.lua").read_text(
        encoding="utf-8"
    )
    for required_text in (
        "CopySave nested callback sequence drift",
        "residual registered callback",
        "residual SRAM bytes",
    ):
        assert required_text in observer


def test_church_raise_observer_names_watchdog_and_case_frame_errors() -> None:
    root = Path(__file__).resolve().parents[2]
    observer = (root / "tools" / "bizhawk" / "church_raise_lifecycle_observer.lua").read_text(
        encoding="utf-8"
    )
    for required_text in (
        "case watchdog exhausted for",
        "ChurchMenu A6 frame balance drift",
        "ChurchMenu A7 stack balance drift",
    ):
        assert required_text in observer


def test_bizhawk_lua_preflight_compiles_observers_and_rejects_syntax_errors(
    tmp_path: Path,
) -> None:
    try:
        _, executable = bizhawk_contract()
    except FileNotFoundError:
        pytest.skip("local BizHawk toolchain is not installed")

    root = Path(__file__).resolve().parents[2]
    for path in sorted((root / "tools" / "bizhawk").glob("*.lua")):
        validate_lua_syntax(path, executable)

    invalid = tmp_path / "reserved-field.lua"
    invalid.write_text("local config = {}\nreturn config.function\n", encoding="utf-8")
    with pytest.raises(ValueError, match=r"expected near 'function'"):
        validate_lua_syntax(invalid, executable)
