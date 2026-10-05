"""Independent reached source spans and field Label joins."""

import itertools
import re

from .text_material_units import font


def text_material_field(
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
):
    programs = {p["id"]: p for p in w.get("programs", [])}
    text_cursor, producer_text = None, None
    required_field, controls = _occurrence_map(), []
    active_producer, span = None, 0
    for e in events:
        try:
            loc = e.get("Program")
            if e["Kind"] == "program-instruction" and loc is not None:
                ins = programs[loc["Program"]]["instructions"][int(loc["Instruction"])]
                if ins.get("op") == "text-cursor":
                    text_cursor = ins["text"]
                elif ins.get("op") == "show-text":
                    active_producer, span = e["Sequence"], 0
                    producer_text = text_cursor
                    # Source controls, not consumer projections, define the spans.
                    text = texts[text_cursor]
                    controls = list(re.finditer(r"\{W[12]\}", text))
                    required_field[active_producer] = dict(
                        producer=active_producer, text=text_cursor, span=0
                    )
                    if text_cursor is not None:
                        text_cursor += 1
            elif (
                e["Kind"] in ("text-w1-accepted", "text-w2-accepted")
                and active_producer is not None
            ):
                if span >= len(controls):
                    check(f"source text acceptance {e['Sequence']}", False)
                    continue
                control = controls[span]
                check(
                    f"source text acceptance {e['Sequence']}",
                    e["Kind"] == "text-w" + control.group()[2] + "-accepted",
                )
                if control.end() < len(texts[producer_text]):
                    span += 1
                    required_field[e["Sequence"]] = dict(
                        producer=active_producer, text=producer_text, span=span
                    )
                else:
                    active_producer = None
        except (KeyError, IndexError, ValueError):
            check(f"missing source text producer {e['Sequence']}", None)
            active_producer = None
    field_rows = itertools.chain(
        actual["samples"],
        (r for r in outcome.get("records", []) if r.get("label") == "outcome-text-input"),
    )
    field_tokens = _occurrence_set()
    for i, row in enumerate(field_rows):
        s = row["state"]
        f = s.get("fieldText")
        if not f:
            continue
        label = s.get("fieldLabel")
        check(f"field font {i}", font(label.get("font") if label else None, 16))
        try:
            token = f["Token"]["Value"]
            required = required_field.get(token)
            if required is None or ascii_map is None or advances is None:
                check(f"missing field lineage/units {i}", None)
                continue
            tid, producer_sequence = required["text"], required["producer"]
            check(f"field producer {i}", None if f.get("Text") is None else f["Text"] == tid)
            active = s["partyLists"]["Active"]
            expected = units(texts[tid], active[0] if active else 0)
            check(f"field units {i}", None if f.get("Units") is None else expected == f["Units"])
            ends = [j for j, u in enumerate(expected) if u["Kind"] in (3, 4)]
            ends.append(len(expected))
            end = ends[required["span"]]
            check(f"field span {i}", None if f.get("End") is None else f["End"] == end)
            projection = "".join(u["Text"] for u in expected[:end])
            check(
                f"field Label {i}",
                None
                if label is None or f.get("Projection") is None
                else s["sessionId"] == session
                and projection == f["Projection"] == s["dialogue"] == label["text"],
            )
            if (
                label
                and label["visible"]
                and (
                    label["visibleCharacters"] < 0
                    or label["visibleCharacters"] >= label["totalCharacters"]
                )
            ):
                field_tokens.add(token)
            result["field"].append(
                dict(
                    sample=i,
                    token=token,
                    text=tid,
                    revision=s["revision"],
                    producerSequence=producer_sequence,
                    span=required["span"],
                    program=event_by_sequence[producer_sequence].get("Program"),
                )
            )
        except (KeyError, IndexError, ValueError):
            check(f"missing field occurrence operand {i}", None)
    check(
        "every logical/source field span mounted",
        True if required_field and all(token in field_tokens for token in required_field) else None,
    )
    result["requiredField"] = _bounded_list(
        dict(token=token, **binding) for token, binding in required_field.items()
    )
