"""Compose ordered checks and source/resource comparisons for one admitted case."""

from collections import Counter

from sf2tool.remake_h4.field_admission import admit_case
from sf2tool.remake_h4.field_projection import compare_lifecycle, compare_projection
from sf2tool.remake_h4.field_services import compare_services
from sf2tool.remake_h4.field_state import compare_state
from sf2tool.remake_h4.field_wait import WaitInput


def service_case(case, profile, checks=None):
    process = case.get("process") or {}
    records = case.get("records") or []
    checks = [] if checks is None else checks
    coverage = Counter()

    def check(name, value, row=None):
        checks.append(dict(name=name, value=value, row=row))

    results, initial, session, programs, initial_npc = admit_case(
        case, profile, records, process, check
    )
    previous = None
    wait = WaitInput()
    for index, row in results:
        wait.consume(records, index, check)
        prior, previous = previous, row.get("after")
        try:
            before, after, facts = row["before"], row["after"], row["facts"]
            events = facts["observations"]
            wait.check_services(records, index, before, events, check)
            compare_state(
                before,
                after,
                facts,
                events,
                prior,
                initial,
                initial_npc,
                programs,
                session,
                index,
                check,
            )
            if not before:
                continue
            compare_lifecycle(before, after, events, records, results, index, check)
            compare_services(before, after, events, programs, profile, coverage, index, check)
        except (KeyError, ValueError, TypeError, IndexError) as error:
            check("missing/outside comparison operands:" + str(error), None, index)
    compare_projection(records, results, session, profile, coverage, check)
    if process["case"].startswith("portrait"):
        for name in (
            "draw-blink",
            "draw-mouth",
            "portrait-unregistered",
            "portrait-typing",
            "portrait-not-typing",
            "eyes-closed",
            "eyes-open",
            "mouth-open",
            "mouth-closed",
            "closed-draw",
        ):
            check("required branch:" + name, coverage[name] > 0)
    check("bounded service count", coverage["service"] <= 240)
    verdict = (
        "FAIL"
        if any(c["value"] is False for c in checks)
        else "Unavailable"
        if any(c["value"] is None for c in checks)
        else "PASS"
    )
    return dict(
        result=verdict,
        scope="single authored source/current mechanism case; not whole field child",
        case=process["case"],
        checks=[c for c in checks if c["value"] is not True],
        passedByRule=dict(Counter(c["name"] for c in checks if c["value"] is True)),
        coverage=dict(coverage),
    )
