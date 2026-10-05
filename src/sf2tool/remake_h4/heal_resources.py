"""MP, HP and EXP command ordering and live resource persistence."""

from sf2tool.remake_h4.heal_checks import match, missing
from sf2tool.remake_h4.heal_evidence import scoped_events


def compare_resources(
    warps, rows, events, effects, resources, owner, ordinal, session, projections, checks
):
    """Check source phase order and all selected deferred/retained live resources."""
    check, one, scene_at = checks.check, checks.one, projections.at
    begin, end = owner.get("preparedRevision"), owner.get("sceneEndResultRevision")
    end_sequence = owner.get("sceneEndSequence")
    lifecycle_rows = [
        w
        for w in warps
        if owner.get("beforeRevision") is not None
        and begin is not None
        and owner["beforeRevision"] < w.get("result", {}).get("revision", -1) < begin
    ] + rows
    for row in lifecycle_rows:
        r = row.get("result") or {}
        check(
            "resource lifecycle state identity",
            match(
                dict(
                    result=dict(sessionId=session),
                    state=dict(
                        sessionId=session,
                        revision=r.get("revision", missing),
                        observationSequence=r.get("observationSequence", missing),
                    ),
                ),
                row,
            ),
            ordinal,
            r.get("revision"),
        )
    resource_boundaries = {}
    for field, (who, initial, final, previous_phase, applied_phase) in resources.items():
        boundary = one(
            "source " + field + " phase boundary",
            [
                dict(w=w, e=e)
                for w, e in events
                if e.get("Kind") == "scene-step-started" and e.get("Detail") == applied_phase
            ],
            ordinal,
        )
        command_row, command = boundary.get("w", {}), boundary.get("e", {})
        boundary_revision = command_row.get("result", {}).get("revision")
        resource_boundaries[field] = boundary_revision
        effect_row, effect = effects[field].get("w", {}), effects[field].get("e", {})
        effect_revision = effect_row.get("result", {}).get("revision")
        check(
            "source " + field + " effect command",
            match(
                boundary_revision if boundary_revision is not None else missing,
                effect_revision if effect_revision is not None else missing,
            ),
            ordinal,
        )
        if boundary_revision is not None:
            check(
                "source " + field + " preceding phase",
                match(previous_phase, scene_at(boundary_revision - 1).get("phase", missing)),
                ordinal,
            )
            check(
                "source " + field + " applied phase",
                match(applied_phase, scene_at(boundary_revision).get("phase", missing)),
                ordinal,
            )
        completed = one(
            "source " + field + " previous command completion",
            [
                e
                for e in scoped_events(command_row, end, end_sequence)
                if e.get("Kind") == "scene-step-completed"
            ],
            ordinal,
        )
        check(
            "source " + field + " completed phase",
            match(previous_phase, completed.get("Detail", missing)),
            ordinal,
        )
        seqs = [e.get("Sequence") for e in (completed, effect, command)]
        check(
            "source " + field + " command effect order",
            None if any(x is None for x in seqs) else seqs[0] < seqs[1] < seqs[2],
            ordinal,
        )
        for row in lifecycle_rows:
            r = row.get("result") or {}
            revision = r.get("revision")
            # The final Submit may already contain another action. Its final state is not
            # a HEAL resource witness; the last in-scope state and effect remain checked.
            if revision == end and (
                r.get("observationSequence") is None or r["observationSequence"] > end_sequence
            ):
                continue
            live = one(
                "live " + field + " lifecycle actor",
                [a for a in row.get("state", {}).get("actors", []) if a.get("id") == who],
                ordinal,
            )
            value = live.get(field, missing)
            if boundary_revision is None:
                candidates = [
                    match(v if v is not None else missing, value) for v in (initial, final)
                ]
                verdict = False if all(v is False for v in candidates) else None
            else:
                expected = initial if revision < boundary_revision else final
                verdict = match(expected if expected is not None else missing, value)
            check("deferred/retained " + field + " lifecycle", verdict, ordinal, revision)
    for earlier, later in (("mp", "hp"), ("mp", "exp"), ("hp", "exp")):
        a, b = resource_boundaries[earlier], resource_boundaries[later]
        check(
            "source " + earlier + " before " + later,
            None if a is None or b is None else a < b,
            ordinal,
        )
