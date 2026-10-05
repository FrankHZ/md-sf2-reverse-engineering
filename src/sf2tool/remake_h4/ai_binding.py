"""Compose AI source/evidence consumers with the accepted seed proof and report."""

import subprocess

from sf2tool.paths import repo_path
from sf2tool.remake_h4.admission_seed import admission_seed_binding
from sf2tool.remake_h4.ai_checks import AiChecks, absent, merge
from sf2tool.remake_h4.ai_decisions import compare_decisions
from sf2tool.remake_h4.ai_evidence import check_state_continuity, select_evidence
from sf2tool.remake_h4.ai_source import source_rules


def ai_consumer_binding(actual, context, source_root):
    """Matched historical callers plus the accepted current seed transport mechanism."""
    context = context or {}
    result = dict(
        value=None,
        checks=[],
        occurrences=[],
        historical=dict(result="FAIL", reason="historical A disconnected seed latch"),
        unknown=[
            "corrected complete route and its new AI choices are not observed",
            "render interpolation/path projection was not captured",
            "original natural first-AI last writer; turn-score/order; other commandsets",
        ],
    )
    producer = "4d1d1b05f143ed872ceca6ff258cfca2b4087d90"
    accepted = "bbf98c8ddbd04500d57165a958f78f86a9ff209b"
    checks = AiChecks(context.get("sessionId", absent))
    result["checks"] = checks.rows
    check, eq = checks.check, checks.eq
    rows, events, owners, census = select_evidence(actual, context, checks, producer)
    source = None
    try:
        source = source_rules(source_root) if source_root else None
    except (OSError, subprocess.CalledProcessError):
        pass
    except (ValueError, AssertionError) as error:
        check("source contradiction", False)
        result["unknown"].append(str(error))
    check("source operands", True if source else None)
    check_state_continuity(actual, context, rows, events, census, source, checks)
    occurrences, unknown = compare_decisions(rows, events, owners, census, source, checks)
    result["occurrences"].extend(occurrences)
    result["unknown"].extend(unknown)
    # Existing admitted seed composition owns original write/caller and exact PR621 TRX.
    seed_context = context.get("seedContext") or {}
    eq("accepted seed correction identity", accepted, seed_context.get("correctionCommit", absent))
    seed_binding = (
        admission_seed_binding(context["seedActual"], seed_context, source_root)
        if context.get("seedActual") and seed_context
        else dict(value=None, checks=[dict(name="seed composition evidence", value=None)])
    )
    result["seedMechanism"] = seed_binding
    check("accepted executed seed mechanism", seed_binding["value"])
    paths = [
        "remake/src/Sf2.Remake.Domain/Battles/Rules/" + name + ".cs"
        for name in (
            "SourceEnemyAi",
            "AiStandbyRules",
            "AiMovementRules",
            "EnemyPhysicalDecision",
            "BattleRandom",
            "PhysicalTargetRules",
            "WeightedMovement",
            "BattleMovement",
        )
    ]
    paths += ["remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs"]
    paths += [
        "remake/src/Sf2.Remake.Application/Runtime/Battles/" + name + ".cs"
        for name in (
            "BattleMovementContinuation",
            "BattleActionCommitter",
            "BattleSceneContinuation",
            "BattleCommandDispatcher",
            "BattleAdvancer",
        )
    ]
    paths += [
        "remake/src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs",
        "remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs",
        "remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs",
        "remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs",
        "remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs",
    ]
    for path in paths:
        try:
            old = subprocess.check_output(
                ["git", "-C", str(repo_path(".")), "show", producer + ":" + path],
                text=True,
                encoding="utf-8",
            )
            accepted_text = subprocess.check_output(
                ["git", "-C", str(repo_path(".")), "show", accepted + ":" + path],
                text=True,
                encoding="utf-8",
            )
            current = repo_path(path).read_text(encoding="utf-8")
            if path.endswith("/BattleAdvancer.cs"):
                # Reviewed intervening delta only adds round-generation diagnostics. Compare
                # the complete queued-actor dispatch/AI delivery/failure continuation below it.
                marker = "                var actor = BattleTurnFlow.QueuedActor(battle);"
                old, accepted_text, current = (
                    text[text.index(marker) :] for text in (old, accepted_text, current)
                )
            if path.endswith("/BattleTurnFlow.cs"):
                # Start/validation through seed and memory initialization are unchanged;
                # generated turn slots below this boundary are a separate obligation.
                marker = "    internal static EngineBattleState GenerateRound("
                old, accepted_text, current = (
                    text[: text.index(marker)] for text in (old, accepted_text, current)
                )
            check(
                "producer/accepted/current consumer mechanism " + path,
                True if old == accepted_text == current else None,
            )
        except (OSError, ValueError, subprocess.CalledProcessError):
            check("consumer mechanism " + path, None)
    result["composition"] = dict(
        historical="matched local state only; historical latch remains FAIL",
        current="accepted PR621 text copy/entry/first AI seam plus unchanged decision consumers",
        excluded="corrected route/order/choices and rendered interpolation",
        reviewedDelta="BattleAdvancer/BattleTurnFlow expose generated turn data; TurnOrderRules "
        "records candidates/draws/unsorted slots. Per-caller AI operands and the unchanged "
        "queued-actor dispatch are compared; turn generation remains a separate obligation.",
    )
    result["value"] = merge([x["value"] for x in result["checks"]])
    passed = {}
    nonpass = []
    for item in result["checks"]:
        if item["value"] is True:
            passed[item["name"]] = passed.get(item["name"], 0) + 1
        else:
            nonpass.append(item)
    result["checks"] = [dict(name=k, value=True, count=v) for k, v in passed.items()] + nonpass
    result["unknown"] = list(dict.fromkeys(result["unknown"]))
    return result
