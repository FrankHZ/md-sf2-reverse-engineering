"""Compose selected reward evidence, source obligations, ledger and return checks."""

from collections import Counter

from sf2tool.remake_h4.reward_checks import RewardChecks, merge, number
from sf2tool.remake_h4.reward_evidence import select_evidence
from sf2tool.remake_h4.reward_ledger import reduce_rewards
from sf2tool.remake_h4.reward_outcome import compare_outcome
from sf2tool.remake_h4.reward_scenes import compare_scenes
from sf2tool.remake_h4.reward_source import load_source
from sf2tool.remake_h4_reference import UPSTREAM


def reward_consumer_binding(actual, context, source_root):
    """Source rewards at retained action operands, through first persistent party state."""
    result = dict(
        value=None,
        checks=[],
        occurrences=[],
        sourceRules=dict(
            upstream=UPSTREAM,
            owner="docs/design/contracts/combat-resolution.md",
            growth="docs/design/contracts/level-up.md",
            spells="docs/design/contracts/spellbook-state.md",
            lifecycle="docs/design/contracts/battle-control-lifecycle.md",
            binding="source initial profile -> observed reward/growth -> first Party.Progress",
        ),
        unknown=[
            "live agility/base-attack are not directly observed; persistence is composed",
            "unreached status, item recovery, spell-learning and defeat branches",
            "turn generation, AI, field services and complete scene presentation",
        ],
    )
    context = context or {}
    checks = RewardChecks(context.get("sessionId"))
    result["checks"] = checks.rows
    selected, events, event_rows, census = select_evidence(actual, context, checks)
    warps = selected["warpRecords"]
    initial = (selected["samples"].get(context.get("initialSample")) or {}).get("state") or {}
    battle = (selected["samples"].get(context.get("battleSample")) or {}).get("state") or {}
    operands, progress, unknown = load_source(initial, battle, source_root, checks)
    result["unknown"].extend(unknown)
    ordered_events = [events[k] for k in sorted(k for k in events if number(k))]
    rewards, occurrences, unknown = compare_scenes(
        warps,
        events,
        event_rows,
        ordered_events,
        context.get("scenes") or [],
        census,
        operands,
        checks,
    )
    result["occurrences"].extend(occurrences)
    result["unknown"].extend(unknown)
    balances, unknown = reduce_rewards(
        warps, events, census, initial, operands, progress, rewards, checks
    )
    result["unknown"].extend(unknown)
    compare_outcome(
        selected, context, ordered_events, event_rows, battle, operands, progress, balances, checks
    )
    result["applicability"] = dict(
        statusEffects="unreached: no status or recovery equipment at selected after-turn calls",
        spellLearning="unreached: no source threshold at observed new level",
        defeat="unreached: leader lives and all placed enemies are dead",
        liveGrowthStats="composed initial source -> effect -> first party only",
        damageExp="source-initial level1/2 allies versus level0 GIZMO; not a general EXP model",
    )
    result["value"] = merge([c["value"] for c in result["checks"]])
    result["unknown"] = list(dict.fromkeys(result["unknown"]))
    # Keep every failure/absence and occurrence. Repeated successful checks are
    # summarized so immutable gate reports fit the selected evidence output budget.
    passed = Counter(c["name"] for c in result["checks"] if c["value"] is True)
    result["checks"] = [c for c in result["checks"] if c["value"] is not True] + [
        dict(name=name, value=True, count=count) for name, count in passed.items()
    ]
    return result
