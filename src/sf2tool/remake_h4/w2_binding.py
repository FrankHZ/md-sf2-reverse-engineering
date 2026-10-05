"""Compose selected W2 input, source continuation, service and validation checks."""

from sf2tool.remake_h4.w2_caller import compare_caller
from sf2tool.remake_h4.w2_checks import W2Checks, merge
from sf2tool.remake_h4.w2_continuation import compare_continuation
from sf2tool.remake_h4.w2_indicator import compare_indicator
from sf2tool.remake_h4.w2_input import compare_input
from sf2tool.remake_h4.w2_selection import admit_context, select_evidence
from sf2tool.remake_h4.w2_service import compare_neutral, compare_service
from sf2tool.remake_h4.w2_source import load_source
from sf2tool.remake_h4.w2_validation import compare_validation
from sf2tool.remake_h4_reference import UPSTREAM


def w2_consumer_binding(actual, context, source_root):
    """PR618's composed predicate on explicitly selected historical A occurrences.

    Keep the independent accepted inventory separate from candidate channels. A
    contradiction in any available edge dominates missing evidence elsewhere.
    """
    result = dict(
        value=None,
        checks=[],
        occurrences=[],
        sourceRules=dict(
            upstream=UPSTREAM,
            owner="docs/design/contracts/dialogue-system.md",
            section="w2-composed-semantic-acceptance",
        ),
        unknown=[
            "original internal accepting-read instruction/time and complete gate bytes",
            "intermediate same-submit host draw/time for the two later-state witnesses",
        ],
    )
    checks = W2Checks()
    result["checks"] = checks.rows
    context, session, inventory = admit_context(context, checks)
    source = load_source(source_root, checks)
    inputs, sample_revisions, sample_indices, records, receipts, neutrals, polls = select_evidence(
        actual, context, inventory, checks
    )
    for poll, owner, accepting in polls:
        ordinal = poll.get("ordinal")
        token = owner.get("token")
        cursor = dict(Program=owner.get("program"), Instruction=owner.get("instruction"))
        before, after, submission, ready_state, ready_evidence, revision = compare_input(
            poll,
            accepting,
            session,
            cursor,
            token,
            ordinal,
            inputs,
            records,
            sample_revisions,
            neutrals,
            checks,
        )
        program = compare_caller(
            owner, cursor, token, ready_state, source.texts, session, ordinal, checks
        )
        events, found = compare_service(submission, before, accepting, ordinal, checks)
        if not accepting:
            compare_neutral(actual, revision, token, cursor, after, ordinal, checks)
            continue
        expected_resume = compare_continuation(owner, after, events, token, source, ordinal, checks)
        later = compare_indicator(
            owner, sample_indices, found, session, program, revision, after, ordinal, checks
        )
        compare_validation(actual, owner, session, revision, receipts, ordinal, checks)
        result["occurrences"].append(
            dict(
                ordinal=ordinal,
                token=token,
                text=owner.get("text"),
                resultRevision=revision,
                indicatorEvidence="later-state composition" if later else "same-submit",
                sourceProgram=program,
                sourceContinuation=expected_resume,
                readyEvidence=ready_evidence,
            )
        )
    result["value"] = merge([c["value"] for c in result["checks"]])
    return result
