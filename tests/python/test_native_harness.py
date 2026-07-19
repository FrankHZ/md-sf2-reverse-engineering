from __future__ import annotations

import re
from pathlib import Path

import pytest

from sf2tool.cli import build_parser, full_verify_requested
from sf2tool.design_contracts import verify_design_contracts
from sf2tool.h2.battle_ai import _direct_target, _parse_source_file
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.research_index import verify_index
from sf2tool.rom import mega_drive_checksum


def test_design_contracts_are_traceable() -> None:
    assert verify_design_contracts()["Status"] == "PASS"


def test_research_index_validates_without_private_inputs() -> None:
    result = verify_index()
    assert result["Status"] == "PASS"
    assert result["Records"] == 640
    assert result["H2Fixtures"] == 34
    assert result["H3Fixtures"] == result["H3FixtureFiles"] == 54
    assert result["AddressBindings"] == 1036
    assert result["IndexedCodeFiles"] == 381
    assert result["IndexedDataFiles"] == 190


def test_mega_drive_checksum_handles_an_odd_trailing_byte() -> None:
    data = bytearray(0x203)
    data[0x200:] = b"\x12\x34\x56"
    assert mega_drive_checksum(bytes(data)) == "6834"


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


def test_growth_refresh_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "growth-refresh"])
    assert args.h3_command == "growth-refresh"
    assert args.timeout_seconds == 60


def test_enemy_gold_has_a_dedicated_narrow_extraction_command() -> None:
    args = build_parser().parse_args(["h2", "enemy-gold"])
    assert args.h2_command == "enemy-gold"
    assert args.output_path is None


def test_enemy_drops_has_a_dedicated_narrow_extraction_command() -> None:
    args = build_parser().parse_args(["h2", "enemy-drops"])
    assert args.h2_command == "enemy-drops"
    assert args.output_path is None


def test_battle_ai_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-ai"])
    assert args.h2_command == "battle-ai"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_battle_scene_engine_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-scene-engine"])
    assert args.h2_command == "battle-scene-engine"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_battle_scene_animations_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-scene-animations"])
    assert args.h2_command == "battle-scene-animations"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_battle_cutscenes_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-cutscenes"])
    assert args.h2_command == "battle-cutscenes"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_common_scripting_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "common-scripting"])
    assert args.h2_command == "common-scripting"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_common_maps_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "common-maps"])
    assert args.h2_command == "common-maps"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_common_stats_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "common-stats"])
    assert args.h2_command == "common-stats"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_common_menus_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "common-menus"])
    assert args.h2_command == "common-menus"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_tech_interrupts_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "tech-interrupts"])
    assert args.h2_command == "tech-interrupts"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_tech_graphics_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "tech-graphics"])
    assert args.h2_command == "tech-graphics"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_tech_interfaces_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "tech-interfaces"])
    assert args.h2_command == "tech-interfaces"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_tech_services_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "tech-services"])
    assert args.h2_command == "tech-services"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_gameflow_core_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "gameflow-core"])
    assert args.h2_command == "gameflow-core"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_special_screens_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "special-screens"])
    assert args.h2_command == "special-screens"
    assert args.output_path is None
    assert not hasattr(args, "rom_path")


def test_remaining_core_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "remaining-core"])
    assert args.h2_command == "remaining-core"
    assert args.output_path is None


def test_battle_global_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-global-data"])
    assert args.h2_command == "battle-global-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_ally_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "ally-data"])
    assert args.h2_command == "ally-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_core_stats_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "core-stats-data"])
    assert args.h2_command == "core-stats-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_battle_cutscene_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-cutscene-data"])
    assert args.h2_command == "battle-cutscene-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_battle_spriteset_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-spriteset-data"])
    assert args.h2_command == "battle-spriteset-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


def test_battle_routing_data_has_a_source_only_inventory_command() -> None:
    args = build_parser().parse_args(["h2", "battle-routing-data"])
    assert args.h2_command == "battle-routing-data"
    assert not hasattr(args, "rom_path")
    assert args.output_path is None


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


def test_kill_exp_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "kill-exp"])
    assert args.h3_command == "kill-exp"
    assert args.timeout_seconds == 75


def test_award_exp_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "award-exp"])
    assert args.h3_command == "award-exp"
    assert args.timeout_seconds == 90


def test_exp_command_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "exp-command"])
    assert args.h3_command == "exp-command"
    assert args.timeout_seconds == 75


def test_gold_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "gold"])
    assert args.h3_command == "gold"
    assert args.timeout_seconds == 60


def test_enemy_drops_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "enemy-drops"])
    assert args.h3_command == "enemy-drops"
    assert args.timeout_seconds == 75


def test_muddle_action_guard_has_a_dedicated_narrow_runtime_command() -> None:
    args = build_parser().parse_args(["h3", "muddle-action-guard"])
    assert args.h3_command == "muddle-action-guard"
    assert args.timeout_seconds == 120


def test_legacy_powershell_surface_does_not_expand() -> None:
    root = Path(__file__).resolve().parents[2]
    scripts = sorted((root / "scripts").rglob("*.ps1"))
    lines = sum(len(path.read_text(encoding="utf-8").splitlines()) for path in scripts)
    assert len(scripts) <= 36
    assert lines <= 4813


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
