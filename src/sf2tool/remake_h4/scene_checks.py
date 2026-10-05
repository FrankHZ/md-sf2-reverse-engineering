"""Scene-specific partial matching, logical clocks and ordered aggregate checks."""

absent = object()


def merge(values):
    return False if False in values else None if None in values else True


def match(want, got=absent):
    if want is absent or got is absent:
        return None
    if isinstance(want, dict):
        return (
            merge([match(v, got.get(k, absent)) for k, v in want.items()])
            if isinstance(got, dict)
            else False
        )
    if isinstance(want, list):
        if not isinstance(got, list):
            return False
        return merge(
            [True if len(want) == len(got) else None if len(got) < len(want) else False]
            + [match(a, b) for a, b in zip(want, got, strict=False)]
        )
    return (
        type(got) is bool and got == want
        if isinstance(want, bool)
        else not isinstance(got, bool) and got == want
    )


def number(n):
    return isinstance(n, (int, float)) and not isinstance(n, bool) and n >= 0 and n == int(n)


def picked(values, index):
    return values[index] if 0 <= index < len(values) else absent


def actor(e, key="Actor"):
    return (e.get(key) or {}).get("Value")


class SceneChecks:
    """One comparison owns the aggregate insertion order and bounded examples."""

    def __init__(self, session):
        self.session = session
        self.rows = {}

    def check(self, name, value, where=None):
        item = self.rows.setdefault(name, dict(name=name, value=True, count=0, examples=[]))
        item["count"] += 1
        item["value"] = merge([item["value"], value])
        if value is not True and len(item["examples"]) < 8:
            item["examples"].append(dict(occurrence=where, value=value))

    def eq(self, name, want, got=absent, where=None):
        self.check(name, match(want, got), where)

    def clocks(self, name, row, where=None):
        self.eq(name + " session", self.session, row.get("sessionId", absent), where)
        self.check(
            name + " nonnegative clocks",
            merge(
                [number(row[k]) if k in row else None for k in ("revision", "observationSequence")]
            ),
            where,
        )

    def precedes(self, name, a, b, where=None):
        self.check(
            name,
            merge(
                [
                    a[k] <= b[k]
                    if number(a.get(k)) and number(b.get(k))
                    else False
                    if k in a and k in b
                    else None
                    for k in ("revision", "observationSequence")
                ]
            ),
            where,
        )

    def identity(self, name, a, b, where=None):
        for k in ("sessionId", "revision", "observationSequence"):
            self.eq(name + " " + k, a.get(k, absent), b.get(k, absent), where)
