"""HEAL partial-value and ordered-event rules; None remains unavailable."""

missing = object()


def merge(values):
    return False if False in values else None if None in values else True


def match(expected, observed=missing):
    if expected is missing or observed is missing or observed is None and expected is not None:
        return None
    if isinstance(expected, dict):
        return (
            merge([match(v, observed.get(k, missing)) for k, v in expected.items()])
            if isinstance(observed, dict)
            else False
        )
    if isinstance(expected, list):
        if not isinstance(observed, list):
            return False
        return merge(
            [
                False
                if len(observed) > len(expected)
                else None
                if len(observed) < len(expected)
                else True
            ]
            + [match(e, a) for e, a in zip(expected, observed, strict=False)]
        )
    if isinstance(expected, bool):
        return type(observed) is bool and expected == observed
    if isinstance(expected, (int, float)) and isinstance(observed, bool):
        return False
    return expected == observed


def ordered_kinds(expected, observed):
    positions = {e["Kind"]: i for i, e in enumerate(expected)}
    indices, values = [], []
    for event in observed:
        if event.get("Kind") is None:
            values.append(None)
            continue
        i = positions.get(event["Kind"])
        if i is None:
            values.append(False)
        else:
            indices.append(i)
            values.append(match(expected[i], event))
    return merge(
        values + [indices == sorted(set(indices)), True if len(indices) == len(expected) else None]
    )


class HealChecks:
    """Ordered checks for one selected HEAL comparison."""

    def __init__(self):
        self.rows = []

    def check(self, name, value, occurrence=None, revision=None):
        self.rows.append(dict(name=name, value=value, occurrence=occurrence, revision=revision))

    def one(self, name, rows, ordinal):
        self.check(name, None if not rows else len(rows) == 1, ordinal)
        return rows[0] if rows else {}
