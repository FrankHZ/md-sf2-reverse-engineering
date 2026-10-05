"""Physical-only value contracts, ordered observations, and clock check recording.

Missing evidence is distinct from explicit null. Known contradictions dominate
missing operands. Do not reuse this matcher for another family without comparing
its subset, list, ordering and missing-value contracts.
"""

absent = object()
CLOCK_KEYS = ("revision", "observationSequence")
IDENTITY_KEYS = ("sessionId", *CLOCK_KEYS)
SNAPSHOT_KEYS = (*IDENTITY_KEYS, "mainSeed")


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
    # JSON booleans are not numbers, despite Python's bool/int equality.
    if isinstance(expected, bool) or isinstance(observed, bool):
        return type(expected) is bool and type(observed) is bool and observed == expected
    return observed == expected


def ordered_match(expected, observed):
    values, cursor = [], 0
    for item in observed:
        found = next(
            (
                i
                for i in range(cursor, len(expected))
                if expected[i].get("Kind") == item.get("Kind")
            ),
            None,
        )
        if found is None:
            values.append(False)
            continue
        if found != cursor:
            values.append(None)
        values.append(match(expected[found], item))
        cursor = found + 1
    if cursor != len(expected):
        values.append(None)
    return merge(values)


def number(value):
    return (
        isinstance(value, (int, float))
        and not isinstance(value, bool)
        and value >= 0
        and value == int(value)
    )


class PhysicalChecks:
    """Own one ordered physical check list and its declared session identity."""

    def __init__(self, session):
        self.session = session
        self.rows = []

    def check(self, name, value, occurrence=None):
        self.rows.append(dict(name=name, value=value, occurrence=occurrence))

    def eq(self, name, expected, observed=absent, occurrence=None):
        self.check(name, match(expected, observed), occurrence)

    def clocks(self, name, snapshot):
        self.eq(name + " session", self.session, snapshot.get("sessionId", absent))
        self.check(
            name + " nonnegative clocks",
            merge([number(snapshot[k]) if k in snapshot else None for k in CLOCK_KEYS]),
        )

    def precedes(self, name, before, after):
        self.check(
            name,
            merge(
                [
                    before[k] <= after[k]
                    if number(before.get(k)) and number(after.get(k))
                    else False
                    if k in before and k in after
                    else None
                    for k in CLOCK_KEYS
                ]
            ),
        )

    def join(self, name, left, right, keys=IDENTITY_KEYS):
        # Same pairwise span/snapshot mechanism used by W2/admission: another missing
        # channel cannot hide a contradiction between two available identities.
        for key in keys:
            self.check(
                name + " " + key, match(left[key], right.get(key, absent)) if key in left else None
            )
