"""Compose executed generation applicability with independently supplied queue consumers."""

from sf2tool.remake_h4.turn_consumer_checks import ConsumerChecks, merge
from sf2tool.remake_h4.turn_dependencies import compare_dependencies
from sf2tool.remake_h4.turn_evidence import compare_evidence
from sf2tool.remake_h4.turn_frontier import replay_frontier
from sf2tool.remake_h4.turn_input_clocks import compare_input_clocks
from sf2tool.remake_h4.turn_installation import compare_installations
from sf2tool.remake_h4.turn_projection import compare_projection
from sf2tool.remake_h4.turn_result_clocks import compare_result_clocks
from sf2tool.remake_h4.turn_selection import select_channels
from sf2tool.remake_h4_reference import UPSTREAM


def turn_order_consumer_binding(actual, context, source_root):
    """Compose accepted executed generation rules with retained queue consumers.

    Retained queues are inputs to the consumer rule, never expected generation.
    Supplied channels must independently cover the declared census and states.
    """
    context = context or {}
    result = dict(
        value=None,
        checks=[],
        frontier=[],
        diagnostics=[],
        historicalDiagnostic="Unavailable",
        sourceRules=dict(
            upstream=UPSTREAM, owner="docs/design/contracts/battle-control-lifecycle.md"
        ),
    )
    checks = ConsumerChecks()
    result["checks"] = checks.rows
    session, generation = compare_dependencies(context, source_root, checks)
    result["generation"] = generation
    indices, channels = select_channels(actual, context, checks)
    compare_result_clocks(channels, indices, session, checks)
    warps, census = compare_input_clocks(channels, indices, context, session, checks)
    queues, expected = compare_installations(context, channels, session, census, checks)
    supplied, publications, required_rows, census_kinds = compare_evidence(
        context, channels, indices, census, expected, queues, session, result["diagnostics"], checks
    )
    if not census:
        result["value"] = merge([c["value"] for c in result["checks"]])
        return result
    initial = (channels["samples"].get((indices.get("samples") or [None])[0]) or {}).get(
        "state"
    ) or {}
    first_control = next((c for c in census if c[3] == "player-control"), None)
    checks.join(
        "initial queue sample joins owning first control result",
        initial,
        (warps.get(first_control[0], {}).get("result") or {}) if first_control else {},
        (indices.get("samples") or [None])[0],
    )
    frontier, terminal, state_at, hp_images = replay_frontier(
        initial,
        context.get("factions") or {},
        queues,
        census,
        expected,
        supplied,
        publications,
        required_rows,
        checks,
    )
    compare_projection(
        context,
        channels,
        indices,
        session,
        queues,
        census,
        expected,
        supplied,
        required_rows,
        census_kinds,
        state_at,
        hp_images,
        checks,
    )
    result["frontier"] = frontier
    result["terminal"] = terminal
    result["value"] = merge([c["value"] for c in result["checks"]])
    return result
