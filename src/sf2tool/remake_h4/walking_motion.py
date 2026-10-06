"""Source and actual walking motion normalized by independent contribution."""

_MOTION_OPERANDS = (
    "x",
    "y",
    "targetX",
    "targetY",
    "velocityX",
    "velocityY",
    "travelX",
    "travelY",
    "accelerationX",
    "accelerationY",
    "speedX",
    "speedY",
    "flagsA",
    "flagsB",
)


def numeric_motion(e):
    """Keep present boolean contradictions before arithmetic erases their type."""
    return not any(isinstance(e.get(key), bool) for key in _MOTION_OPERANDS)


def movement(x, y, dx, dy, vx, vy, tx, ty, ax, ay, sx, sy, flags_a, flags_b):
    def sign(value):
        return None if value is None else (value > 0) - (value < 0)

    def difference(a, b):
        return None if a is None or b is None else a - b

    def tiles(value):
        return None if value is None else value / 384

    def bit(value, mask):
        return None if value is None else bool(value & mask)

    # Missing operands affect only their own normalized contribution.
    remaining = [difference(dx, x), difference(dy, y)]
    active = [None if d is None else d != 0 for d in remaining]
    return dict(
        activeAxes=active,
        direction=[sign(d) for d in remaining],
        velocityDirection=[
            None if moving is None else sign(v) if moving else 0
            for moving, v in zip(active, (vx, vy), strict=True)
        ],
        travelTiles=[tiles(tx), tiles(ty)],
        remainingTiles=[tiles(None if d is None else abs(d)) for d in remaining],
        accelerationSteps=[tiles(ax), tiles(ay)],
        configuredSpeed=[tiles(sx), tiles(sy)],
        acceleration=[bit(flags_a, 1), bit(flags_a, 2)],
        deceleration=[bit(flags_a, 4), bit(flags_a, 8)],
        obstructable=bit(flags_a, 128),
        mapCollision=bit(flags_a, 64),
        entityCollision=bit(flags_a, 32),
        autoFacing=bit(flags_b, 64),
    )


def actual_movement(e):
    return movement(
        *(e.get(key) for key in _MOTION_OPERANDS[:12]),
        int(e["flagsA"]) if e.get("flagsA") is not None else None,
        int(e["flagsB"]) if e.get("flagsB") is not None else None,
    )
