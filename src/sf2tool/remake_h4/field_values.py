"""Strict field values and partial predecessor joins."""


def _field_equal(expected, actual):
    """Observation JSON keeps booleans distinct from integral numeric values."""
    if isinstance(expected, dict):
        return (
            isinstance(actual, dict)
            and expected.keys() == actual.keys()
            and all(_field_equal(v, actual[k]) for k, v in expected.items())
        )
    if isinstance(expected, list):
        return (
            isinstance(actual, list)
            and len(expected) == len(actual)
            and all(_field_equal(a, b) for a, b in zip(expected, actual, strict=True))
        )
    return expected == actual and (
        isinstance(actual, bool)
        if isinstance(expected, bool)
        else not isinstance(actual, bool)
        if isinstance(expected, (int, float))
        else True
    )


def _field_join(left, right):
    """Join partial observations: a missing leaf cannot erase a known contradiction."""
    if isinstance(left, dict) and isinstance(right, dict):
        values = [_field_join(left[k], right[k]) for k in left.keys() & right.keys()]
        if left.keys() != right.keys():
            values.append(None)
    elif isinstance(left, list) and isinstance(right, list):
        values = [_field_join(a, b) for a, b in zip(left, right, strict=False)]
        if len(left) != len(right):
            values.append(None)
    else:
        return _field_equal(left, right)
    return False if False in values else None if None in values else True
