"""Compose selected W1 input, displayed token, service and retained-state comparisons."""

from sf2tool.remake_h4.w1_checks import W1Checks, merge
from sf2tool.remake_h4.w1_continuation import compare_continuation
from sf2tool.remake_h4.w1_envelopes import compare_envelopes
from sf2tool.remake_h4.w1_history import admit_history
from sf2tool.remake_h4.w1_input import compare_input
from sf2tool.remake_h4.w1_npc import compare_npc
from sf2tool.remake_h4.w1_retained import compare_retained
from sf2tool.remake_h4.w1_selection import admit_context, select_records
from sf2tool.remake_h4.w1_services import compare_portrait, compare_preamble
from sf2tool.remake_h4.w1_source import load_source
from sf2tool.remake_h4.w1_text import compare_text
from sf2tool.remake_h4_reference import UPSTREAM


def w1_consumer_binding(actual, context, source_root):
    """Bind the retained W1 cohort to source rules, whole inputs and live consumers.

    Context selects identities only. Source supplies token/continuation and RNG
    rules; actual snapshots supply operands. Missing evidence never supplies a
    default gate, and a contradiction dominates an unrelated missing operand.
    """
    result = dict(
        value=None,
        checks=[],
        occurrences=[],
        sourceRules=dict(
            upstream=UPSTREAM,
            owner="docs/design/contracts/dialogue-system.md",
            section="reached-w1-consumer-binding",
        ),
        unknown=[
            "original live service bytes/timing beyond the named text483 accepting read",
            "intermediate portrait clocks/typewriting restoration before a source-bound close",
            "branch-flag truth outside the W1 resume boundary",
        ],
    )
    checks = W1Checks()
    result["checks"] = checks.rows
    context = context or {}
    supplement, session, owners, polls = admit_context(actual, context, checks)
    inputs, records, states = select_records(actual, supplement, context, checks)
    compare_envelopes(inputs, records, polls, session, checks)
    source = load_source(source_root, checks)
    installations, npc_history_events = admit_history(
        actual, supplement, context, session, source, checks
    )
    for poll in polls:
        ordinal, accepting = poll.get("ordinal"), poll.get("accepting")
        owner = checks.one(
            "one independent token occurrence",
            [o for o in owners if o.get("token") == poll.get("token")],
            ordinal,
        )
        cur, token = owner.get("cursor") or {}, poll.get("token")
        before, after, submit, ready, post = compare_input(
            poll, owner, inputs, records, states, session, checks
        )
        suppressed, field = compare_text(
            poll, owner, ready, owners, polls, states, source, session, checks
        )
        events, seed, byte = compare_preamble(submit, before, accepting, ordinal, checks)
        seed, npc_attempts = compare_npc(
            ready, post, poll, suppressed, installations, npc_history_events, source, seed, checks
        )
        open_portrait, expected_portrait, read_positions = compare_portrait(
            ready, after, events, seed, ordinal, checks
        )
        compare_continuation(
            accepting,
            cur,
            token,
            ready,
            after,
            events,
            open_portrait,
            read_positions,
            source,
            ordinal,
            checks,
        )
        compare_retained(
            poll,
            polls,
            actual,
            supplement,
            context,
            states,
            before,
            after,
            submit,
            post,
            field,
            events,
            open_portrait,
            expected_portrait,
            byte,
            checks,
        )
        result["occurrences"].append(
            dict(
                ordinal=ordinal,
                token=token,
                text=owner.get("text"),
                accepting=accepting,
                npcAttempts=npc_attempts,
                copyEvidence="same-submit" if post else "later-state composition",
            )
        )
    result["value"] = merge([c["value"] for c in result["checks"]])
    return result
