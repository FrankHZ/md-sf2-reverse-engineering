"""Compose region checks, execution identities and repeated-read continuity in order."""

from sf2tool.remake_h4.map_delivery import compare_delivery
from sf2tool.remake_h4.map_draw import draw_cells
from sf2tool.remake_h4.map_layout import compare_identity, compare_layout, compare_use_facts
from sf2tool.remake_h4.map_projection import admit_projection


def compare_redraws(role, samples, check):
    by_label = {s.get("label"): s["layout"] for s in samples if "layout" in s}
    repeats = (
        (
            ("door-opened", "door-repeat-read"),
            ("roof-restored", "roof-repeat-read"),
            ("roof-cleared", "roof-clear-repeat"),
            ("flag-off-load", "flag-off-repeat"),
        )
        if role == "house"
        else (("school-roof-restored", "school-roof-repeat"),)
        if role == "school"
        else ()
    )
    for before, after in repeats:
        a, b = by_label.get(before), by_label.get(after)
        check(
            "read-only redraw leaves snapshot unchanged",
            None
            if a is None or b is None
            else False
            if any(a[k] != b[k] for k in a.keys() & b.keys() - {"draw"})
            else None
            if a.keys() != b.keys()
            else True,
            after,
        )


def compare_witnesses(
    context, maps, sources, wanted, school_entities, session, root, witnesses, check
):
    groups = context.get("witnesses", [])
    check("no unexpected witness role", all(g.get("role") in sources for g in groups))
    seen, role_sessions, identities = set(), {}, {}
    for group in groups:
        role = group.get("role")
        if role not in sources:
            continue
        samples = group.get("samples", [])
        compare_delivery(role, samples, sources[role], check)
        initial = [s["state"] for s in samples if s.get("label", "").startswith("layout-start")]
        for sample in samples:
            if "layout" not in sample:
                continue
            label, layout = sample.get("label"), sample["layout"]
            key = role, label
            if key not in wanted:
                check("unexpected region witness", False, label)
                continue
            check("unique class/state witness", key not in seen, label)
            seen.add(key)
            compare_use_facts(layout, maps[sources[role]], sources[role], label, check)
            try:
                map_id, source = sources[role], maps[sources[role]]
                expected = compare_layout(layout, wanted[key], map_id, source, label, check)
                draw = layout["draw"]
                check("actual draw present", True if draw else None, label)
                if not draw:
                    continue
                compare_identity(
                    layout, draw, session, role, label, role_sessions, identities, check
                )
                projected, actor_population_complete = admit_projection(
                    sample, layout, draw, role, label, initial, school_entities, check
                )
                detail = draw_cells(
                    projected,
                    expected,
                    source,
                    root,
                    actor_population_complete=actor_population_complete,
                )
                check("complete executed coordinate/resource multiset", detail["value"], label)
                witnesses.append(dict(role=role, label=label, **detail))
            except KeyError:
                check("witness operand absent", None, label)
            except (ValueError, IndexError, TypeError, OSError):
                check("malformed or contradictory witness", False, label)
        compare_redraws(role, samples, check)
    check(
        "every required source class/state has actual witness",
        True if set(wanted) <= seen else None,
    )
    return [list(k) for k in sorted(set(wanted) - seen)]
