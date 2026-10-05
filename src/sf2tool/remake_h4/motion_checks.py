"""Ordered occurrence checks and missing/contradiction aggregation."""


def motion_checks(_bounded_list):
    result = dict(
        operation=None, consumer=None, checks=_bounded_list(), occurrences=_bounded_list()
    )

    def check(family, name, value, token=None):
        result["checks"].append(dict(family=family, name=name, value=value, token=token))

    def common(name, value):
        for family in ("operation", "consumer"):
            check(family, name, value)

    def aggregate(values):
        return False if False in values else None if None in values or not values else True

    def operand(family, token, name, operands, predicate):
        # Evaluate independently: missing elsewhere cannot hide this contradiction.
        check(
            family, name, None if any(x is None for x in operands) else predicate(*operands), token
        )

    return result, check, common, aggregate, operand
