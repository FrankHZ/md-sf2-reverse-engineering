"""W1-specific partial matching and one binding's ordered check log."""


def merge(values):
    return False if False in values else None if None in values else True


_OMITTED = object()


class W1Checks:
    def __init__(self):
        self.rows = []
        self.absent = object()

    def match(self, expected, observed=_OMITTED):
        absent = self.absent
        if observed is _OMITTED:
            observed = absent
        if expected is absent or observed is absent or observed is None and expected is not None:
            return None
        if isinstance(expected, dict):
            if not isinstance(observed, dict):
                return False
            return merge([self.match(v, observed.get(k, absent)) for k, v in expected.items()])
        if isinstance(expected, bool):
            return type(observed) is bool and expected == observed
        if isinstance(expected, (int, float)) and isinstance(observed, bool):
            return False
        return expected == observed

    def check(self, name, value, ordinal=None):
        self.rows.append(dict(name=name, value=value, ordinal=ordinal))

    def one(self, name, candidates, ordinal=None):
        self.check(name, None if not candidates else len(candidates) == 1, ordinal)
        return candidates[0] if candidates else {}
