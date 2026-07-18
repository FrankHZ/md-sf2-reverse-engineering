from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path
from typing import Any

from sf2tool.design_contracts import verify_design_contracts
from sf2tool.h3.battle_exp import verify_battle_exp_level_up
from sf2tool.h3.growth import verify_growth
from sf2tool.h3.rng import verify_rng
from sf2tool.h3.spell_boost import verify_spell_boost
from sf2tool.h3.spell_damage import verify_spell_damage, verify_spell_summon
from sf2tool.h3.spell_desoul import verify_spell_desoul
from sf2tool.h3.spell_healing import verify_spell_healing
from sf2tool.h3.spell_mp import verify_spell_mp_absorb
from sf2tool.h3.spell_slow import verify_spell_slow
from sf2tool.h3.spell_status import verify_spell_status
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


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="sf2", description="Shining Force II research harness")
    commands = parser.add_subparsers(dest="command", required=True)

    verify_parser = commands.add_parser("verify", help="run the repository verification rails")
    _add_local_paths(verify_parser)
    verify_parser.add_argument("--skip-rebuild", action="store_true")
    verify_parser.add_argument("--skip-extraction", action="store_true")
    verify_parser.add_argument("--skip-runtime", action="store_true")
    verify_parser.add_argument(
        "--quick",
        action="store_true",
        help="run commit-level Python, contract, index, ROM, and toolchain gates only",
    )

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
    h3_battle_exp = h3_commands.add_parser(
        "battle-exp", help="verify natural battle EXP-to-level-up behavior"
    )
    _add_local_paths(h3_battle_exp)
    h3_battle_exp.add_argument("--timeout-seconds", type=int, default=75)
    h3_spell_damage = h3_commands.add_parser(
        "spell-damage", help="verify BLAZE 2 damage across all four fire-resistance settings"
    )
    _add_local_paths(h3_spell_damage)
    h3_spell_damage.add_argument("--timeout-seconds", type=int, default=75)
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
    return parser


def dispatch(args: argparse.Namespace) -> None:
    if args.command == "verify":
        verify(
            rom_path=args.rom_path,
            upstream_path=args.upstream_path,
            skip_rebuild=args.skip_rebuild,
            skip_extraction=args.skip_extraction,
            skip_runtime=args.skip_runtime,
            quick=args.quick,
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
    elif args.command == "h3" and args.h3_command == "battle-exp":
        print_record(
            verify_battle_exp_level_up(
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
