"""Compose selected HEAL source, resource and fairy consumer comparisons."""

from sf2tool.remake_h4.heal_checks import HealChecks, match, merge
from sf2tool.remake_h4.heal_evidence import SceneProjections, select_channels, select_occurrence
from sf2tool.remake_h4.heal_opportunities import compare_opportunities
from sf2tool.remake_h4.heal_preparation import compare_preparation
from sf2tool.remake_h4.heal_resources import compare_resources
from sf2tool.remake_h4.heal_source import load_tables
from sf2tool.remake_h4.heal_work import compare_work
from sf2tool.remake_h4_reference import UPSTREAM


def heal_consumer_binding(actual, context, source_root):
    """Selected PRST HEAL1 consumers under the accepted PR618 logical clock."""
    result = dict(
        value=None,
        checks=[],
        occurrences=[],
        sourceRules=dict(
            upstream=UPSTREAM,
            owner="docs/design/contracts/spell-resolution.md#confirmed-heal-1-subset",
            clock="docs/research/map3-messenger-acceptance.md#legal-heal-recovery-window-completion",
            fairy="disasm/code/gameflow/battle/battlescenes/animation/healingfairy.asm:1A848; "
            "animation/update/healingfairy.asm:1C53E; updatespellanimation.asm",
            motion="disasm/code/common/tech/graphics/graphics_1.asm:sub_179C; "
            "disasm/data/tech/spellanimations.asm:table_1840",
        ),
        historical=dict(
            prepared04="FAIL: CP2059..2091 original3 versus logical2; "
            "later recovery-text seed mismatch",
            scope="retained failure, not a mandatory third logical opportunity",
        ),
        unknown=[
            "original interrupted PC/CPU timing and complete historical window gates",
            "whole historical A carried RNG/AI trajectory and complete battle-scene fidelity",
        ],
    )
    checks = HealChecks()
    result["checks"] = checks.rows
    check = checks.check
    context = context or {}
    check("accepted selected scope", match("retained-keyboard-A-heal", context.get("scope")))
    session = context.get("sessionId")
    check("independent session", bool(session) or None)
    cohort = context.get("occurrences") or []
    check(
        "selected occurrence inventory",
        None
        if not cohort
        else [(o.get("actor"), o.get("expectedTarget"), o.get("targetSprite")) for o in cohort]
        == [("ally-1", "ally-0", 0), ("ally-1", "ally-2", 2), ("ally-1", "ally-1", 1)],
    )
    quarter, cast, idle = load_tables(source_root, checks)
    warps, scenes, inputs = select_channels(actual, context, checks)
    for ordinal, owner in enumerate(cohort):
        rows, by_revision, inventory_complete = select_occurrence(warps, owner, ordinal, checks)
        scalar, resources, effects, prepared, events = compare_preparation(
            warps, rows, by_revision, inputs, owner, ordinal, session, checks
        )
        projections = SceneProjections(scenes, owner, by_revision, session, ordinal, checks)
        compare_resources(
            warps, rows, events, effects, resources, owner, ordinal, session, projections, checks
        )
        work = compare_opportunities(
            rows,
            prepared,
            inputs,
            owner,
            ordinal,
            session,
            quarter,
            projections,
            inventory_complete,
            scalar,
            checks,
        )
        compare_work(work, owner, ordinal, events, cast, idle, checks)
        result["occurrences"].append(
            dict(
                ordinal=ordinal,
                actor=owner.get("actor"),
                target=owner.get("expectedTarget"),
                opportunities=work["opportunities"],
                fairyDraws=work["draws"],
            )
        )
    result["value"] = merge([c["value"] for c in result["checks"]])
    return result
