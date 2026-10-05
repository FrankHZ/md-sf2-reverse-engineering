"""Opening-specific values, uniqueness and ordered report checks."""


class OpeningChecks:
    """Ordered opening checks with this proof's missing and boolean semantics."""

    def __init__(self):
        self.rows = []
        self.absent = object()

    @staticmethod
    def merge(values):
        return False if False in values else None if None in values else True

    def match(self, want, got):
        absent = self.absent
        if want is absent or got is absent or got is None and want is not None:
            return None
        if isinstance(want, dict):
            return (
                self.merge([self.match(v, got.get(k, absent)) for k, v in want.items()])
                if isinstance(got, dict)
                else False
            )
        if isinstance(want, bool):
            return type(got) is bool and got == want
        if isinstance(want, (int, float)) and isinstance(got, bool):
            return False
        return got == want

    def check(self, name, value):
        self.rows.append(dict(name=name, value=value))

    def one(self, name, rows):
        self.check(name + " unique", None if not rows else len(rows) == 1)
        return rows[0] if rows else {}
