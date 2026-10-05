"""Draw identity, physical geometry, culling and actual effect phases."""

import struct

from sf2tool.remake_h4.motion_wait import entity


def motion_projection(
    draws, subject, token, ins, role, effect, semantic_phases, used, phase_use, check, operand
):
    for b in draws:
        s = b["state"]
        p = s.get("cameraProjection") or {}
        if subject is None:
            # Palette/white nodes remain real consumers before field
            # geometry exists (first after-program fade) and after mount.
            cue = s.get("presentationCue") or {}
            if cue.get("token") == token:
                used.append(True)
                for name, value, expected in (
                    ("drawn fade kind", cue.get("kind"), ins["kind"]),
                    ("drawn fade resource", cue.get("resource"), ins["resource"]),
                ):
                    operand(
                        "consumer",
                        token,
                        name,
                        [value],
                        lambda a, expected=expected: a == expected,
                    )
                operand(
                    "consumer",
                    token,
                    "actual fade drawn in same occurrence",
                    [cue.get("kind"), cue.get("resource"), cue.get("ownerVisible")],
                    lambda a, b, c, ins=ins: (
                        a == ins["kind"] and b == ins["resource"] and c is True
                    ),
                )
            continue
        if p.get("token") != token:
            continue
        operand(
            "consumer",
            token,
            "actual draw session",
            [p.get("sessionId"), s.get("sessionId")],
            lambda a, b: a == b,
        )
        operand(
            "consumer",
            token,
            "actual draw revision",
            [p.get("revision"), s.get("revision")],
            lambda a, b: a <= b,
        )
        if subject is not None:
            logical = entity(s, subject)
            actor = next((a for a in p.get("actors") or [] if a.get("entity") == subject), None)
            if logical is not None and logical.get("Visible") is False:
                used.append(True)
                check(
                    "consumer",
                    "logically hidden subject has no visible draw",
                    actor is None or actor.get("visible") is False,
                    token,
                )
                continue
            if logical is None or actor is None:
                check("consumer", "actual subject projection operand", None, token)
                continue
            used.append(True)
            operand(
                "consumer",
                token,
                "physical subject slot",
                [logical.get("slot"), actor.get("slot")],
                lambda a, b: a == b,
            )
            for dimension, viewport_size in (("width", 320), ("height", 192)):
                operand(
                    "consumer",
                    token,
                    "accepted viewport " + dimension,
                    [p.get(dimension), p.get("scale")],
                    lambda a, b, viewport_size=viewport_size: abs(a - viewport_size * b) < 0.002,
                )
                operand(
                    "consumer",
                    token,
                    "accepted actor extent " + dimension,
                    [actor.get(dimension), p.get("scale")],
                    lambda a, b: abs(a - 24 * b) < 0.002,
                )
            shift = actor.get("shiverOffsetX")
            for axis in ("x", "y"):
                operand(
                    "consumer",
                    token,
                    "logical pose projected " + axis,
                    [
                        logical.get(axis),
                        actor.get(axis),
                        actor.get("origin" + axis.upper()),
                        p.get(axis),
                        p.get("scale"),
                        shift if axis == "x" else 0,
                    ],
                    lambda a, b, c, d, scale, shift, axis=axis: (
                        abs(b - (d + (a / 16 + (shift if axis == "x" else 0) - c) * scale)) < 0.002
                    ),
                )

            def intersects(x, y, w, h, px, py, pw, ph, visible):
                # Rect2 uses float32, including its edge additions.
                def as_float32(number):
                    return struct.unpack("f", struct.pack("f", number))[0]

                x, y, w, h, px, py, pw, ph = map(as_float32, (x, y, w, h, px, py, pw, ph))
                return visible is (
                    as_float32(x + w) > px
                    and as_float32(y + h) > py
                    and x < as_float32(px + pw)
                    and y < as_float32(py + ph)
                )

            operand(
                "consumer",
                token,
                "viewport geometry and legitimate culling",
                [
                    actor.get("x"),
                    actor.get("y"),
                    actor.get("width"),
                    actor.get("height"),
                    p.get("x"),
                    p.get("y"),
                    p.get("width"),
                    p.get("height"),
                    actor.get("visible"),
                ],
                intersects,
            )
            if semantic_phases(s) and effect == "shiver":
                cue = p.get("cue") or {}
                operand(
                    "consumer",
                    token,
                    "active shiver draw subject",
                    [cue.get("gesture")],
                    lambda a, subject=subject: a == subject,
                )
                operand(
                    "consumer",
                    token,
                    "active shiver draw flag",
                    [cue.get("shivering")],
                    lambda a: a is True,
                )
            if semantic_phases(s) and effect in ("mosaic-in", "mosaic-out"):
                cue = p.get("cue") or {}
                operand(
                    "consumer",
                    token,
                    "active mosaic draw subject",
                    [cue.get("mosaic")],
                    lambda a, subject=subject: a == subject,
                )
                operand(
                    "consumer",
                    token,
                    "active mosaic draw direction",
                    [cue.get("mosaicOut")],
                    lambda a, effect=effect: a is (effect == "mosaic-out"),
                )
            if actor.get("visible") is True:
                if role == "nod" and actor.get("gesture") is True:
                    phase_use.update(semantic_phases(s))
                elif (
                    effect == "shiver"
                    and (p.get("cue") or {}).get("shivering") is True
                    and (p.get("cue") or {}).get("gesture") == subject
                ):
                    phase_use.add(actor.get("shiverOffsetX"))
                elif (
                    effect in ("mosaic-in", "mosaic-out")
                    and (p.get("cue") or {}).get("mosaic") == subject
                ):
                    phase_use.add(actor.get("mosaicBlock"))
            if role == "nod":
                nod = s.get("nod") or {}
                operand(
                    "consumer",
                    token,
                    "nod gesture subject installed",
                    [actor.get("gesture")],
                    lambda a: a is True,
                )
                operand(
                    "consumer",
                    token,
                    "nod animation held",
                    [logical.get("animationCounter")],
                    lambda a: a == 255,
                )
                operand(
                    "consumer",
                    token,
                    "bound nod source phase",
                    [
                        nod.get("Elapsed"),
                        actor.get("lowered"),
                    ],
                    lambda age, low: low is (10 <= age < 30),
                )
            elif ins.get("resource") == "shiver":
                cue = p.get("cue") or {}
                operand(
                    "consumer",
                    token,
                    "shiver legal offset",
                    [shift],
                    lambda a, cue=cue: a in (-1, 1) if cue.get("shivering") else a == 0,
                )

                def shiver(age, offset, cue=cue):
                    if not cue.get("shivering"):
                        return offset == 0
                    # Godot JSON rounds the retained double age; compare
                    # its serialization interval at a phase boundary.
                    return offset in {
                        1 if int(max(0, age + d) * 60 / 5) % 2 == 0 else -1 for d in (-1e-14, 1e-14)
                    }

                operand(
                    "consumer",
                    token,
                    "actual shiver alternating phase",
                    [cue.get("elapsed"), shift],
                    shiver,
                )
            elif ins.get("resource") in ("mosaic-in", "mosaic-out"):
                cue = p.get("cue") or {}
                if cue.get("mosaic") == subject:
                    operand(
                        "consumer",
                        token,
                        "mosaic legal block",
                        [actor.get("mosaicBlock")],
                        lambda a: a in (1, 2, 4, 6, 8),
                    )

                    def mosaic(age, block, ins=ins):
                        age = 0.5 - age if ins["resource"] == "mosaic-out" else age
                        return block in {
                            8
                            if a < 0.1
                            else 6
                            if a < 0.2
                            else 4
                            if a < 0.3
                            else 2
                            if a < 0.4
                            else 1
                            for a in (age - 1e-14, age + 1e-14)
                        }

                    operand(
                        "consumer",
                        token,
                        "actual mosaic finite phase",
                        [cue.get("elapsed"), actor.get("mosaicBlock")],
                        mosaic,
                    )
