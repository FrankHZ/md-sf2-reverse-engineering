from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path
from typing import Any

from sf2tool.design_contracts import verify_design_contracts
from sf2tool.h2.battle_ai import verify_battle_ai_inventory
from sf2tool.h2.enemy_drops import verify_enemy_item_drops
from sf2tool.h2.enemy_gold import verify_enemy_gold
from sf2tool.h3.after_turn import verify_after_turn_status_lifecycle
from sf2tool.h3.award_exp import verify_award_exp_randomization
from sf2tool.h3.battle_ai_action import verify_battle_ai_action_choice
from sf2tool.h3.battle_exp import verify_battle_exp_level_up
from sf2tool.h3.enemy_curse import verify_enemy_curse_suppression
from sf2tool.h3.enemy_drops import verify_enemy_item_drop_behavior
from sf2tool.h3.exp_command import verify_exp_command_boundaries
from sf2tool.h3.gold import verify_gold_boundaries
from sf2tool.h3.growth import (
    verify_growth,
    verify_initialization_prowess,
    verify_level_up_refresh,
)
from sf2tool.h3.kill_exp import verify_kill_exp_level_differences
from sf2tool.h3.muddle_action_guard import verify_muddle_action_guard
from sf2tool.h3.muddle_confusion import verify_muddle_confusion
from sf2tool.h3.rng import verify_rng
from sf2tool.h3.spell_attack import verify_spell_attack
from sf2tool.h3.spell_boost import verify_spell_boost
from sf2tool.h3.spell_damage import verify_spell_damage, verify_spell_summon
from sf2tool.h3.spell_desoul import verify_spell_desoul
from sf2tool.h3.spell_detox import verify_spell_detox
from sf2tool.h3.spell_dispel import verify_spell_dispel
from sf2tool.h3.spell_exp import verify_spell_damage_exp
from sf2tool.h3.spell_healing import (
    verify_spell_aura,
    verify_spell_healing,
    verify_spell_healing_exp,
)
from sf2tool.h3.spell_mp import verify_spell_mp_absorb
from sf2tool.h3.spell_muddle import verify_spell_muddle, verify_spell_muddle1
from sf2tool.h3.spell_silence import verify_spell_silence_gate
from sf2tool.h3.spell_slow import verify_spell_slow
from sf2tool.h3.spell_status import verify_spell_status
from sf2tool.h3.stat_clamps import verify_stat_clamp_boundaries
from sf2tool.harness import verify
from sf2tool.legacy import run_powershell
from sf2tool.output import print_json, print_record
from sf2tool.paths import repo_path
from sf2tool.research_index import index_rows, index_summary, query_index, verify_index
from sf2tool.rom import verify_rom

DEFAULT_ROM = repo_path("local/roms/sf2-us.bin")
DEFAULT_UPSTREAM = repo_path("local/upstream/SF2DISASM")


def _path(value: str) -> Path:
    return Path(value)


def _add_local_paths(parser: argparse.ArgumentParser, *, rom: bool = True) -> None:
    if rom:
        parser.add_argument("--rom-path", type=_path, default=DEFAULT_ROM)
    parser.add_argument("--upstream-path", type=_path, default=DEFAULT_UPSTREAM)


def _print_rows(rows: list[dict[str, Any]]) -> None:
    if not rows:
        return
    columns = list(rows[0])
    widths = {
        column: max(len(column), *(len(str(row[column])) for row in rows)) for column in columns
    }
    print("  ".join(f"{column:<{widths[column]}}" for column in columns))
    print("  ".join("-" * widths[column] for column in columns))
    for row in rows:
        print("  ".join(f"{str(row[column]):<{widths[column]}}" for column in columns))


def full_verify_requested(args: argparse.Namespace) -> bool:
    if args.quick:
        return False
    return args.full or args.skip_rebuild or args.skip_extraction or args.skip_runtime


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="sf2", description="Shining Force II research harness")
    commands = parser.add_subparsers(dest="command", required=True)

    verify_parser = commands.add_parser("verify", help="run the repository verification rails")
    _add_local_paths(verify_parser)
    verify_parser.add_argument(
        "--skip-rebuild", action="store_true", help="full profile: omit the H1 rebuild"
    )
    verify_parser.add_argument(
        "--skip-extraction", action="store_true", help="full profile: omit H2 extraction rails"
    )
    verify_parser.add_argument(
        "--skip-runtime", action="store_true", help="full profile: omit H3 runtime rails"
    )
    verify_profile = verify_parser.add_mutually_exclusive_group()
    verify_profile.add_argument(
        "--full",
        action="store_true",
        help="enable the milestone profile with H1, H2, and H3 rails",
    )
    verify_profile.add_argument("--quick", action="store_true", help=argparse.SUPPRESS)

    init_parser = commands.add_parser("init", help="initialize ignored local research inputs")
    init_parser.add_argument("--rom-path", type=_path, required=True)

    rom_parser = commands.add_parser("rom", help="inspect or verify the ROM baseline")
    rom_commands = rom_parser.add_subparsers(dest="rom_command", required=True)
    rom_verify = rom_commands.add_parser("verify")
    rom_verify.add_argument("--rom-path", type=_path, default=DEFAULT_ROM)

    index_parser = commands.add_parser("research-index", aliases=["index"])
    index_commands = index_parser.add_subparsers(dest="index_command", required=True)
    index_test = index_commands.add_parser("test")
    index_test.add_argument("--upstream-path", type=_path, default=DEFAULT_UPSTREAM)
    index_list = index_commands.add_parser("list")
    index_list.add_argument("--query")
    index_list.add_argument("--subsystem")
    index_list.add_argument("--status", choices=("confirmed", "inferred", "unknown"))
    index_list.add_argument("--fixture")
    index_list.add_argument("--summary", action="store_true")
    index_list.add_argument("--json", action="store_true")

    design_parser = commands.add_parser("design-contracts")
    design_parser.add_subparsers(dest="design_command", required=True).add_parser("test")

    h2_parser = commands.add_parser("h2", help="run a narrow deterministic extraction rail")
    h2_commands = h2_parser.add_subparsers(dest="h2_command", required=True)
    h2_enemy_gold = h2_commands.add_parser(
        "enemy-gold", help="extract and byte-compare the enemy gold table"
    )
    _add_local_paths(h2_enemy_gold)
    h2_enemy_gold.add_argument("--output-path", type=_path)
    h2_enemy_drops = h2_commands.add_parser(
        "enemy-drops", help="extract and byte-compare the enemy item drop table"
    )
    _add_local_paths(h2_enemy_drops)
    h2_enemy_drops.add_argument("--output-path", type=_path)
    h2_battle_ai = h2_commands.add_parser(
        "battle-ai", help="inventory the complete battle AI source subtree"
    )
    _add_local_paths(h2_battle_ai, rom=False)
    h2_battle_ai.add_argument("--output-path", type=_path)

    h3_parser = commands.add_parser("h3", help="run a narrow emulator-backed fixture")
    h3_commands = h3_parser.add_subparsers(dest="h3_command", required=True)
    h3_rng = h3_commands.add_parser("rng", help="verify base and debug-aware RNG behavior")
    h3_rng.add_argument("--rom-path", type=_path, default=DEFAULT_ROM)
    h3_rng.add_argument("--timeout-seconds", type=int, default=60)
    h3_growth = h3_commands.add_parser(
        "growth", help="verify stat-gain and complete level-up behavior"
    )
    _add_local_paths(h3_growth)
    h3_growth.add_argument("--timeout-seconds", type=int, default=60)
    h3_growth_refresh = h3_commands.add_parser(
        "growth-refresh", help="verify level-up derived-stat and equipment refresh behavior"
    )
    _add_local_paths(h3_growth_refresh)
    h3_growth_refresh.add_argument("--timeout-seconds", type=int, default=60)
    h3_growth_prowess = h3_commands.add_parser(
        "growth-prowess", help="verify the HEAL 3 initialization prowess special case"
    )
    _add_local_paths(h3_growth_prowess)
    h3_growth_prowess.add_argument("--timeout-seconds", type=int, default=60)
    h3_stat_clamps = h3_commands.add_parser(
        "stat-clamps", help="verify stat cap and byte-underflow clamp boundaries"
    )
    _add_local_paths(h3_stat_clamps)
    h3_stat_clamps.add_argument("--timeout-seconds", type=int, default=60)
    h3_enemy_curse = h3_commands.add_parser(
        "enemy-curse", help="verify cursed equipment does not mark enemies as cursed"
    )
    _add_local_paths(h3_enemy_curse)
    h3_enemy_curse.add_argument("--timeout-seconds", type=int, default=60)
    h3_battle_exp = h3_commands.add_parser(
        "battle-exp", help="verify natural battle EXP-to-level-up behavior"
    )
    _add_local_paths(h3_battle_exp)
    h3_battle_exp.add_argument("--timeout-seconds", type=int, default=75)
    h3_kill_exp = h3_commands.add_parser(
        "kill-exp", help="verify kill EXP level-difference brackets and promotion offset"
    )
    _add_local_paths(h3_kill_exp)
    h3_kill_exp.add_argument("--timeout-seconds", type=int, default=75)
    h3_award_exp = h3_commands.add_parser(
        "award-exp", help="verify final EXP halving, randomization, and minimum award"
    )
    _add_local_paths(h3_award_exp)
    h3_award_exp.add_argument("--timeout-seconds", type=int, default=90)
    h3_exp_command = h3_commands.add_parser(
        "exp-command", help="verify EXP storage, threshold, and single-level command boundaries"
    )
    _add_local_paths(h3_exp_command)
    h3_exp_command.add_argument("--timeout-seconds", type=int, default=75)
    h3_gold = h3_commands.add_parser(
        "gold", help="verify gold addition, cap, and unsigned-carry boundaries"
    )
    _add_local_paths(h3_gold)
    h3_gold.add_argument("--timeout-seconds", type=int, default=60)
    h3_enemy_drops = h3_commands.add_parser(
        "enemy-drops", help="verify rare, repeated-flag, and guaranteed enemy item drops"
    )
    _add_local_paths(h3_enemy_drops)
    h3_enemy_drops.add_argument("--timeout-seconds", type=int, default=75)
    h3_muddle_confusion = h3_commands.add_parser(
        "muddle-confusion", help="verify the MUDDLE counter/level-2 confusion truth table"
    )
    _add_local_paths(h3_muddle_confusion)
    h3_muddle_confusion.add_argument("--timeout-seconds", type=int, default=75)
    h3_muddle_action_guard = h3_commands.add_parser(
        "muddle-action-guard",
        help="verify muddled ally/enemy protected-target and self-target action guards",
    )
    _add_local_paths(h3_muddle_action_guard)
    h3_muddle_action_guard.add_argument("--timeout-seconds", type=int, default=120)
    h3_battle_ai_action = h3_commands.add_parser(
        "battle-ai-action", help="verify batched battle AI action choice and target tie-breaks"
    )
    _add_local_paths(h3_battle_ai_action)
    h3_battle_ai_action.add_argument("--timeout-seconds", type=int, default=120)
    h3_spell_damage = h3_commands.add_parser(
        "spell-damage", help="verify BLAZE 2 damage across all four fire-resistance settings"
    )
    _add_local_paths(h3_spell_damage)
    h3_spell_damage.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_exp = h3_commands.add_parser(
        "spell-exp", help="verify attack-spell damage EXP brackets, kill bonus, and cap"
    )
    _add_local_paths(h3_spell_exp)
    h3_spell_exp.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_summon = h3_commands.add_parser(
        "spell-summon", help="verify promoted DAO target-count power division"
    )
    _add_local_paths(h3_spell_summon)
    h3_spell_summon.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_healing = h3_commands.add_parser(
        "spell-healing", help="verify HEAL 1 recovery, EXP award, and command replay"
    )
    _add_local_paths(h3_spell_healing)
    h3_spell_healing.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_healing_exp = h3_commands.add_parser(
        "spell-healing-exp", help="verify healing EXP eligibility, minimum, and cap"
    )
    _add_local_paths(h3_spell_healing_exp)
    h3_spell_healing_exp.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_aura = h3_commands.add_parser(
        "spell-aura", help="verify AURA radius targets, all-allies filtering, and EXP accumulation"
    )
    _add_local_paths(h3_spell_aura)
    h3_spell_aura.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_detox = h3_commands.add_parser(
        "spell-detox", help="verify DETOX level masks, ineffective branch, and curse unequip"
    )
    _add_local_paths(h3_spell_detox)
    h3_spell_detox.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_attack = h3_commands.add_parser(
        "spell-attack", help="verify ATTACK 1 fresh application and recast failure"
    )
    _add_local_paths(h3_spell_attack)
    h3_spell_attack.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_muddle = h3_commands.add_parser(
        "spell-muddle", help="verify MUDDLE 2 across all four status-resistance settings"
    )
    _add_local_paths(h3_spell_muddle)
    h3_spell_muddle.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_muddle1 = h3_commands.add_parser(
        "spell-muddle1", help="verify MUDDLE 1 resistance bypass, recast, and level-2 guard"
    )
    _add_local_paths(h3_spell_muddle1)
    h3_spell_muddle1.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_status = h3_commands.add_parser(
        "spell-status", help="verify SLEEP 1 across all four status-resistance settings"
    )
    _add_local_paths(h3_spell_status)
    h3_spell_status.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_desoul = h3_commands.add_parser(
        "spell-desoul", help="verify DESOUL instant death, kill EXP, and gold replay"
    )
    _add_local_paths(h3_spell_desoul)
    h3_spell_desoul.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_mp = h3_commands.add_parser(
        "spell-mp", help="verify SPOIT random MP drain, clamp, EXP, and command replay"
    )
    _add_local_paths(h3_spell_mp)
    h3_spell_mp.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_boost = h3_commands.add_parser(
        "spell-boost", help="verify BOOST 1 fresh application, recast, stats, and replay"
    )
    _add_local_paths(h3_spell_boost)
    h3_spell_boost.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_slow = h3_commands.add_parser(
        "spell-slow", help="verify SLOW 1 resistance thresholds, stats, EXP, and replay"
    )
    _add_local_paths(h3_spell_slow)
    h3_spell_slow.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_dispel = h3_commands.add_parser(
        "spell-dispel",
        help="verify DISPEL 1 spell counting, resistance, recast, EXP, and replay",
    )
    _add_local_paths(h3_spell_dispel)
    h3_spell_dispel.add_argument("--timeout-seconds", type=int, default=90)
    h3_spell_silence = h3_commands.add_parser(
        "spell-silence", help="verify SILENCE blocks affected spells before effect dispatch"
    )
    _add_local_paths(h3_spell_silence)
    h3_spell_silence.add_argument("--timeout-seconds", type=int, default=90)
    h3_after_turn = h3_commands.add_parser(
        "after-turn", help="verify status expiry, continuation, and final stat refresh"
    )
    _add_local_paths(h3_after_turn)
    h3_after_turn.add_argument("--timeout-seconds", type=int, default=150)
    return parser


def dispatch(args: argparse.Namespace) -> None:
    if args.command == "verify":
        verify(
            rom_path=args.rom_path,
            upstream_path=args.upstream_path,
            skip_rebuild=args.skip_rebuild,
            skip_extraction=args.skip_extraction,
            skip_runtime=args.skip_runtime,
            full=full_verify_requested(args),
        )
    elif args.command == "init":
        run_powershell("Initialize-LocalResearch.ps1", ("-RomPath", args.rom_path))
    elif args.command == "rom":
        print_record(verify_rom(args.rom_path))
    elif args.command in {"research-index", "index"}:
        if args.index_command == "test":
            upstream = args.upstream_path if args.upstream_path.exists() else None
            print_record(verify_index(upstream))
        else:
            records = query_index(
                query=args.query,
                subsystem=args.subsystem,
                status=args.status,
                fixture=args.fixture,
            )
            result: Any = index_summary(records) if args.summary else index_rows(records)
            if args.json:
                print_json(result)
            elif args.summary:
                print_record(result)
            else:
                _print_rows(result)
    elif args.command == "design-contracts":
        print_record(verify_design_contracts())
    elif args.command == "h2" and args.h2_command == "enemy-gold":
        print_record(
            verify_enemy_gold(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "enemy-drops":
        print_record(
            verify_enemy_item_drops(
                args.rom_path,
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h2" and args.h2_command == "battle-ai":
        print_record(
            verify_battle_ai_inventory(
                args.upstream_path,
                output_path=args.output_path,
            )
        )
    elif args.command == "h3" and args.h3_command == "rng":
        print_record(verify_rng(args.rom_path, timeout_seconds=args.timeout_seconds))
    elif args.command == "h3" and args.h3_command == "growth":
        print_record(
            verify_growth(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "growth-refresh":
        print_record(
            verify_level_up_refresh(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "growth-prowess":
        print_record(
            verify_initialization_prowess(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "stat-clamps":
        print_record(
            verify_stat_clamp_boundaries(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "enemy-curse":
        print_record(
            verify_enemy_curse_suppression(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "battle-exp":
        print_record(
            verify_battle_exp_level_up(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "kill-exp":
        print_record(
            verify_kill_exp_level_differences(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "award-exp":
        print_record(
            verify_award_exp_randomization(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "exp-command":
        print_record(
            verify_exp_command_boundaries(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "gold":
        print_record(
            verify_gold_boundaries(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "enemy-drops":
        print_record(
            verify_enemy_item_drop_behavior(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "muddle-confusion":
        print_record(
            verify_muddle_confusion(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "muddle-action-guard":
        print_record(
            verify_muddle_action_guard(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "battle-ai-action":
        print_record(
            verify_battle_ai_action_choice(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-damage":
        print_record(
            verify_spell_damage(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-exp":
        print_record(
            verify_spell_damage_exp(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-summon":
        print_record(
            verify_spell_summon(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-healing":
        print_record(
            verify_spell_healing(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-healing-exp":
        print_record(
            verify_spell_healing_exp(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-aura":
        print_record(
            verify_spell_aura(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-detox":
        print_record(
            verify_spell_detox(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-attack":
        print_record(
            verify_spell_attack(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-muddle":
        print_record(
            verify_spell_muddle(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-muddle1":
        print_record(
            verify_spell_muddle1(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-status":
        print_record(
            verify_spell_status(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-desoul":
        print_record(
            verify_spell_desoul(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-mp":
        print_record(
            verify_spell_mp_absorb(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-boost":
        print_record(
            verify_spell_boost(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-slow":
        print_record(
            verify_spell_slow(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-dispel":
        print_record(
            verify_spell_dispel(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "spell-silence":
        print_record(
            verify_spell_silence_gate(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    elif args.command == "h3" and args.h3_command == "after-turn":
        print_record(
            verify_after_turn_status_lifecycle(
                args.rom_path,
                args.upstream_path,
                timeout_seconds=args.timeout_seconds,
            )
        )
    else:
        raise AssertionError(f"unhandled command: {args.command}")


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    try:
        dispatch(parser.parse_args(argv))
    except (OSError, ValueError, RuntimeError, subprocess.CalledProcessError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0
