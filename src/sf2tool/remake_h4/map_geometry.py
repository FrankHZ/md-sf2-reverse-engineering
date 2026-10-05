"""Float32 viewport arithmetic and positive-area clipping."""

import struct


def f(value):
    return struct.unpack("f", struct.pack("f", value))[0]


def add(a, b):
    return f(f(a) + f(b))


def sub(a, b):
    return f(f(a) - f(b))


def mul(a, b):
    return f(f(a) * f(b))


def div(a, b):
    return f(f(a) / f(b))


def rect(row):
    return tuple(f(row[k]) for k in ("x", "y", "width", "height"))


def intersect(a, b):
    x, y = max(a[0], b[0]), max(a[1], b[1])
    right = min(add(a[0], a[2]), add(b[0], b[2]))
    bottom = min(add(a[1], a[3]), add(b[1], b[3]))
    return (x, y, sub(right, x), sub(bottom, y)) if right > x and bottom > y else None
