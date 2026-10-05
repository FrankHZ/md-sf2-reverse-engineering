"""AI-specific missing/subsequence contracts and ordered check recording.

Unlike physical matching, a shorter list may be a retained subsequence. Preserve
that distinction and the AI null/number/clock rules rather than sharing a matcher.
"""

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
        if len(got) < len(want):
            # A retained subsequence proves absence, not a shifted-value contradiction.
            remaining = iter(want)
            for item in got:
                if not any(match(candidate, item) is not False for candidate in remaining):
                    return False
            return None
        return merge(
            [True if len(want) == len(got) else None if len(got) < len(want) else False]
            + [match(x, y) for x, y in zip(want, got, strict=False)]
        )
    return got == want and (
        type(got) is bool
        if isinstance(want, bool)
        else not isinstance(got, bool)
        if isinstance(want, (int, float))
        else True
    )


def number(x):
    return isinstance(x, (int, float)) and not isinstance(x, bool) and x >= 0 and x == int(x)


def event_clock(e):
    return dict(revision=e.get("Revision"), observationSequence=e.get("Sequence"))


def who(e, key="Actor"):
    return (e.get(key) or {}).get("Value")


class AiChecks:
    """One AI comparison's ordered evidence checks and declared session."""

    def __init__(self, session):
        self.session = session
        self.rows = []

    def check(self, name, value, occurrence=None):
        self.rows.append(dict(name=name, value=value, occurrence=occurrence))

    def eq(self, name, want, got=absent, occurrence=None):
        self.check(name, match(want, got), occurrence)

    def clocks(self, name, row):
        self.eq(name + " session", self.session, row.get("sessionId", absent))
        for k in ("revision", "observationSequence"):
            self.check(name + " " + k, number(row[k]) if row.get(k) is not None else None)

    def before(self, name, left, right):
        self.check(
            name,
            merge(
                [
                    left[k] <= right[k]
                    if number(left.get(k)) and number(right.get(k))
                    else None
                    if left.get(k) is None or right.get(k) is None
                    else False
                    for k in ("revision", "observationSequence")
                ]
            ),
        )
