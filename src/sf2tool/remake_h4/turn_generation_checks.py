"""Strict generation matching and ordered per-round comparison records."""

import json


def merge(values):
    return False if False in values else None if None in values else True


class GenerationChecks:
    def __init__(self):
        self.rows = []

    def check(self, name, value, **identity):
        self.rows.append(dict(name=name, value=value, **identity))

    def match(self, expected, observed):
        if observed is None and expected is not None:
            return None
        if isinstance(expected, dict):
            if not isinstance(observed, dict):
                return False
            return merge(
                [self.match(v, observed[k]) if k in observed else None for k, v in expected.items()]
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
        return type(expected) is type(observed) and expected == observed

    def ordered_records_match(self, expected, observed, keys):
        if observed is None:
            return None
        if not isinstance(observed, list):
            return False

        def key(row):
            return json.dumps([row[k] for k in keys], sort_keys=True)

        positions = {key(row): index for index, row in enumerate(expected)}
        indices, values = [], []
        for row in observed:
            if any(row.get(k) is None for k in keys):
                values.append(None)
                continue
            index = positions.get(key(row))
            if index is None:
                values.append(False)
                continue
            indices.append(index)
            values.append(self.match(expected[index], row))
        values.extend(
            [
                indices == sorted(indices) and len(indices) == len(set(indices)),
                True if len(indices) == len(expected) else None,
            ]
        )
        return merge(values)
