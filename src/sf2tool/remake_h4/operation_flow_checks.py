"""Five-family ordered check ledger and missing/contradiction reduction."""


def operation_flow_checks(_bounded_list):
    names = (
        "taken route/setup/caller branch operands and occurrence",
        "dialogue speaker/control-token occurrence and choice effect",
        "route roster/flag writes at their source branch",
        "warp destination/setup initialization before field release",
        "before/after operation effects and shared-tail return pairing",
    )
    result = dict(
        values={}, checks=_bounded_list(), programs=_bounded_list(), warps=_bounded_list()
    )

    def check(family, name, value, sequence=None):
        result["checks"].append(dict(family=family, name=name, value=value, sequence=sequence))

    def common(name, value):
        for family in names:
            check(family, name, value)

    def finish():
        for family in names:
            values = _bounded_list(c["value"] for c in result["checks"] if c["family"] == family)
            result["values"][family] = (
                False if False in values else None if None in values or not values else True
            )
        return result

    return names, result, check, common, finish
