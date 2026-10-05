"""Accepted choice, flag and return ordering with independent flag state."""


def operation_flow_choices(op, seq, ins, executed, ordered, anchor, state_of, names, check):
    if op == "yes-no":
        stop = next(
            (
                x["Sequence"]
                for x, i in executed
                if x["Sequence"] > seq and i and i["op"] == "yes-no"
            ),
            float("inf"),
        )
        accepted = next(
            (x for x in ordered if seq < x["Sequence"] < stop and x["Kind"] == "choice-accepted"),
            None,
        )
        flag_event = next(
            (
                x
                for x in ordered
                if seq < x["Sequence"] < stop and x["Kind"] == "choice-result-flag"
            ),
            None,
        )
        returned = next(
            (x for x in ordered if seq < x["Sequence"] < stop and x["Kind"] == "choice-returned"),
            None,
        )
        check(
            names[1],
            "accepted choice/flag/return source occurrence",
            None
            if not accepted or not flag_event or not returned
            else seq < accepted["Sequence"] < flag_event["Sequence"] < returned["Sequence"]
            and int(flag_event["Detail"]) == ins["flag"]
            and returned["Detail"] == accepted["Detail"],
            seq,
        )
        after = anchor(returned["Sequence"], "flags", False) if returned else None
        check(
            names[1],
            "choice flag effect in independent full state",
            None
            if after is None or accepted is None
            else (ins["flag"] in state_of(after)["flags"]) == (accepted["Detail"] == "yes"),
            seq,
        )
