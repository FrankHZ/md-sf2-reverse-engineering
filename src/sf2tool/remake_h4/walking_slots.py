"""R1 pointer/template, phase and consumed walking gate joins by physical slot."""

from .field_values import _field_equal
from .walking_motion import actual_movement, movement, numeric_motion


def matches(expected, value):
    return None if value is None else _field_equal(expected, value)


def walking_slots(
    raw_record,
    ref,
    source_ok,
    map3,
    expected_actions,
    next_wait,
    initial,
    admitted,
    later,
    consumed,
    no_reset,
    delta,
    parts,
    motion_parts,
    anchors,
    combined,
):
    for ordinal, (slot, character, center) in enumerate(
        ((5, 130, (20, 13, 3)), (6, 131, (18, 10, 1)), (8, 133, (12, 9, 1)))
    ):
        values = parts[slot]
        try:
            entity = next(e for e in raw_record["facts"]["entities"] if e["physical"] == slot)
            raw = bytes(entity["bytes"])

            def word(offset, signed=False, data=raw):
                return int.from_bytes(data[offset : offset + 2], "big", signed=signed)

            pointer = int.from_bytes(raw[20:24], "big")
            base = 0xFF5600 + ordinal * 50
            offset = pointer - base
            projected = next(
                (e for e in ref["inherited"].get("entities", []) if e["physical"] == slot), {}
            )
            bound_parts = [source_ok, len(raw) == 32, offset in (0, 40)]
            index_binding = None
            try:
                index_binding = raw_record["facts"]["entityIndexBytes"][character - 96] == slot
            except (KeyError, IndexError):
                index_binding = None
            bound_parts.append(index_binding)
            for key, value in (
                ("actionScript", pointer),
                ("waitTimer", raw[31]),
                ("x", word(0)),
                ("y", word(2)),
                ("destinationX", word(12)),
                ("destinationY", word(14)),
            ):
                bound_parts.append(matches(value, projected.get(key)))
            values.extend(bound_parts)
            motion_parts.extend(bound_parts)
            content_ok = None
            try:
                template = next(e for e in map3["entities"] if e["id"] == f"entity-{character}")
                content_ok = _field_equal(
                    expected_actions
                    + [
                        dict(op="random-walk", x=center[0], y=center[1], radius=center[2]),
                        dict(op="wait", ticks=next_wait),
                        dict(op="jump", instruction=8),
                    ],
                    template["actions"],
                )
            except (KeyError, StopIteration):
                pass
            values[2] = content_ok
            motion_parts.append(content_ok)
            observed = next((e for e in initial.get("entities", []) if e["slot"] == slot), {})
            admission = next((e for e in admitted.get("entities", []) if e["slot"] == slot), {})
            motion_parts.extend(numeric_motion(state) for state in (observed, admission))
            cursor = 0 if offset == 0 else 9
            moving = (word(0), word(2)) != (word(12), word(14))
            for state in (observed, admission):
                for key, value in (
                    ("id", f"entity-{character}"),
                    ("actionCursor", cursor),
                    ("moving", moving),
                ):
                    values.append(matches(value, state.get(key)))
            expected = movement(
                word(0),
                word(2),
                word(12),
                word(14),
                word(4, True),
                word(6, True),
                word(8),
                word(10),
                raw[24],
                raw[25],
                raw[26],
                raw[27],
                raw[28],
                raw[29],
            )

            actual_motion = actual_movement(observed)
            for state_motion in (actual_motion, actual_movement(admission)):
                for key, value in expected.items():
                    actual_value = state_motion[key]
                    if isinstance(value, list):
                        motion_parts.extend(
                            matches(v, a) for v, a in zip(value, actual_value, strict=True)
                        )
                    else:
                        motion_parts.append(matches(value, actual_value))
            anchors["slots"][slot] = dict(
                base=base,
                offset=offset,
                character=character,
                expectedCursor=cursor,
                expectedMoving=moving,
                expectedMotion=expected,
                actualMotion=actual_motion,
            )
            gate_parts = [None]
            if later:
                after = next((e for e in consumed.get("entities", []) if e["slot"] == slot), {})
                gate_parts = [no_reset, matches(f"entity-{character}", after.get("id"))]
                if offset == 0:
                    gate_parts.extend(
                        (
                            raw[31] >= 30,
                            None
                            if after.get("actionCursor") is None
                            else after["actionCursor"] in (8, 9),
                        )
                    )
                else:
                    gate_parts.append(matches(cursor, after.get("actionCursor")))
                    gate_parts.append(matches(moving, after.get("moving")))
                    gate_parts.append(
                        matches(0 if moving else raw[31] + delta, after.get("waitTimer"))
                    )
                    if moving:
                        gate_parts.extend(
                            not isinstance(after.get(key), bool)
                            for key in ("x", "y", "targetX", "targetY")
                        )
                        gate_parts.extend(
                            (
                                matches(word(12), after.get("targetX")),
                                matches(word(14), after.get("targetY")),
                            )
                        )
                        progress = None
                        try:
                            progress = abs(after["x"] - after["targetX"]) + abs(
                                after["y"] - after["targetY"]
                            ) < abs(word(0) - word(12)) + abs(word(2) - word(14))
                        except KeyError:
                            progress = None
                        gate_parts.append(progress)
            gate = combined(gate_parts)
            if later:
                anchors["slots"][slot]["consumedGate"] = gate
            values.append(gate)
            motion_parts.append(gate)
        except (KeyError, IndexError, StopIteration):
            values.append(None)
            motion_parts.append(None)
