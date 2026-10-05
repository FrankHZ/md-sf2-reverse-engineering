"""Consumer matching, grouped bounded examples and clock identity comparisons."""


def merge(values):
    return False if False in values else None if None in values else True


def number(value):
    # Godot JSON retains long clocks as integral floats. Booleans, fractions,
    # negative values and values outside the application long domain are wrong.
    return type(value) in (int, float) and 0 <= value <= 2**63 - 1 and value % 1 == 0


CLOCK_KEYS = ("revision", "observationSequence")
_OMITTED = object()


class ConsumerChecks:
    def __init__(self):
        self.rows = []
        self.groups = {}
        self.absent = object()

    def match(self, expected, observed=_OMITTED):
        absent = self.absent
        if observed is _OMITTED:
            observed = absent
        if expected is absent or observed is absent or observed is None and expected is not None:
            return None
        if isinstance(expected, dict):
            return (
                False
                if not isinstance(observed, dict)
                else merge([self.match(v, observed.get(k, absent)) for k, v in expected.items()])
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
                + [self.match(e, a) for e, a in zip(expected, observed, strict=False)]
            )
        if isinstance(expected, (int, float)) and not isinstance(expected, bool):
            return type(observed) in (int, float) and observed == expected
        return observed == expected and (not isinstance(expected, bool) or type(observed) is bool)

    def check(self, name, value, **identity):
        key = name, value
        if key not in self.groups:
            self.groups[key] = dict(name=name, value=value, count=0, examples=[])
            self.rows.append(self.groups[key])
        group = self.groups[key]
        group["count"] += 1
        if identity and len(group["examples"]) < 8:
            group["examples"].append(identity)

    def clocks(self, name, snapshot, index, keys=CLOCK_KEYS):
        self.check(
            name + " clock domain",
            merge([None if snapshot.get(k) is None else number(snapshot[k]) for k in keys]),
            index=index,
        )

    def precedes(self, name, before, after, index):
        self.check(
            name,
            merge(
                [
                    before[k] <= after[k]
                    if number(before.get(k)) and number(after.get(k))
                    else None
                    if before.get(k) is None or after.get(k) is None
                    else False
                    for k in CLOCK_KEYS
                ]
            ),
            index=index,
        )

    def join(self, name, before, after, index):
        self.check(
            name,
            merge(
                [
                    self.match(before.get(k, self.absent), after.get(k, self.absent))
                    for k in ("sessionId", *CLOCK_KEYS)
                ]
            ),
            index=index,
        )
