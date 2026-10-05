"""Ordered displayed-text material binding with caller-owned readers and storage."""

from .text_material_battle import text_material_battle
from .text_material_field import text_material_field
from .text_material_source import text_material_source
from .text_material_units import unit_reader


def text_material_binding(
    actual,
    outcome,
    selection,
    source_root,
    *,
    read,
    _bounded_list,
    _occurrence_map,
    _occurrence_set,
    _occurrence_dict,
    _bounded_sorted,
):
    """Full reached text material/modern Label join, independent of rendered selectors."""
    result = dict(
        value=None,
        checks=_bounded_list(),
        field=_bounded_list(),
        battle=_bounded_list(),
        boundary="modern configured font",
    )
    if not selection or source_root is None:
        return result

    def check(name, value):
        result["checks"].append(dict(name=name, value=value))

    w, texts, names, enemy_names, ascii_map, advances = text_material_source(
        selection, source_root, read, check
    )
    session = actual["samples"][0]["state"]["sessionId"]
    records = actual.get("warpRecords", [])
    events, seen = _bounded_list(), _occurrence_set()
    for ri, r in enumerate(records):
        for e in r["result"].get("observations", []):
            if e["Sequence"] not in seen:
                events.append(dict(record=ri, **e))
                seen.add(e["Sequence"])
    events.sort(key=lambda e: e["Sequence"])
    event_by_sequence = _occurrence_dict((e["Sequence"], e) for e in events)

    units = unit_reader(names, ascii_map, advances)
    text_material_field(
        actual,
        outcome,
        w,
        texts,
        ascii_map,
        advances,
        events,
        event_by_sequence,
        session,
        units,
        result,
        check,
        _bounded_list,
        _occurrence_map,
        _occurrence_set,
    )
    text_material_battle(
        actual,
        events,
        event_by_sequence,
        records,
        session,
        names,
        enemy_names,
        texts,
        result,
        check,
        _bounded_list,
        _occurrence_map,
        _occurrence_set,
        _bounded_sorted,
    )
    values = _bounded_list(c["value"] for c in result["checks"])
    result["value"] = False if False in values else None if None in values or not values else True
    return result
