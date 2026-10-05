"""Reward-family value/clock matching and ordered check recording.

Keep this family's count, list, null and missing-value contracts local. Similar
syntax in another family does not establish interchangeable evidence semantics.
"""

absent = object()


def merge(values):
    return False if False in values else None if None in values else True


def match(expected, observed=absent):
    if expected is absent or observed is absent:
        return None
    if isinstance(expected, dict):
        return (
            merge([match(v, observed.get(k, absent)) for k, v in expected.items()])
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
            + [match(x, y) for x, y in zip(expected, observed, strict=False)]
        )
    if (
        isinstance(expected, (int, float))
        and not isinstance(expected, bool)
        and isinstance(observed, bool)
    ):
        return False
    return observed == expected and (not isinstance(expected, bool) or type(observed) is bool)


def number(value):
    return (
        isinstance(value, (int, float))
        and not isinstance(value, bool)
        and value >= 0
        and value == int(value)
    )


def actor(event, key="Actor"):
    return (event.get(key) or {}).get("Value")


class RewardChecks:
    """One reward comparison's ordered checks and declared session."""

    def __init__(self, session):
        self.session = session
        self.rows = []

    def check(self, name, value, occurrence=None):
        self.rows.append(dict(name=name, value=value, occurrence=occurrence))

    def eq(self, name, expected, observed=absent, occurrence=None):
        self.check(name, match(expected, observed), occurrence)

    def clocks(self, name, row):
        self.eq(name + " session", self.session, row.get("sessionId", absent))
        self.check(
            name + " clocks",
            merge(
                [number(row[k]) if k in row else None for k in ("revision", "observationSequence")]
            ),
        )

    def precedes(self, name, left, right):
        self.check(
            name,
            merge(
                [
                    left[k] <= right[k]
                    if number(left.get(k)) and number(right.get(k))
                    else False
                    if k in left and k in right
                    else None
                    for k in ("revision", "observationSequence")
                ]
            ),
        )

    def join(self, name, left, right):
        for key in ("sessionId", "revision", "observationSequence"):
            self.check(
                name + " " + key, match(left[key], right.get(key, absent)) if key in left else None
            )

    def count(self, name, expected, observed, occurrence=None):
        self.check(
            name,
            True if expected == observed else None if observed < expected else False,
            occurrence,
        )
