"""Executed generation applicability and the seven tested/current dependency reads."""

import subprocess

from sf2tool.paths import repo_path
from sf2tool.remake_h4.turn_generation import turn_order_binding
from sf2tool.remake_h4_reference import UPSTREAM


def compare_dependencies(context, source_root, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    check(
        "explicit composed scope",
        match("retained-keyboard-A-turn-composed", context.get("scope", absent)),
    )
    check(
        "original producer",
        match("4d1d1b05f143ed872ceca6ff258cfca2b4087d90", context.get("producer", absent)),
    )
    check("source revision", match(UPSTREAM, context.get("upstream", absent)))
    session = context.get("sessionId")
    check("independent session", True if isinstance(session, str) and session else None)
    generation = turn_order_binding(context.get("generationActual") or {}, source_root)
    generation_rows = (context.get("generationActual") or {}).get("rounds")
    check(
        "bounded accepted generation selection",
        None if not generation_rows else len(generation_rows) <= 3,
    )
    check("accepted actual generation/source leg", generation["value"])
    tested = context.get("generationCommit")
    check(
        "accepted tested generation revision",
        match("742e4306dc706f5ddd1fce1fa1168c70cf889477", tested),
    )
    for path in (
        "remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs",
        "remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs",
        "remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs",
        "remake/src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs",
        "remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs",
        "remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs",
        "remake/src/Sf2.Remake.Application/Runtime/SessionContract.cs",
    ):
        name = "tested/current dependency " + path
        try:
            old = subprocess.check_output(
                ["git", "-C", str(repo_path(".")), "show", f"{tested}:{path}"]
            )
        except (OSError, subprocess.CalledProcessError) as error:
            check(name, None, reason="tested dependency read failed", error=type(error).__name__)
            continue
        try:
            current = repo_path(path).read_bytes().replace(b"\r\n", b"\n")
        except OSError as error:
            check(name, None, reason="current dependency read failed", error=type(error).__name__)
        else:
            # Byte differences invalidate proof reuse; consumer contradictions still run.
            if old == current:
                check(name, True)
            else:
                check(
                    name, None, reason="dependency differs; executed proof requires renewed review"
                )
    return session, generation
